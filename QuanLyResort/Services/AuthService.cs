using Microsoft.IdentityModel.Tokens;
using QuanLyResort.Models;
using QuanLyResort.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuanLyResort.Services;

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
}

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IAuditService _auditService;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _auditService = auditService;
    }

    public async Task<(bool Success, string? Token, User? User)> LoginAsync(string emailOrUsername, string password, string? role = null)
    {
        // Tìm user theo email HOẶC username
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);
        
        Console.WriteLine($"[LoginAsync DEBUG] EmailOrUsername: {emailOrUsername}");
        Console.WriteLine($"[LoginAsync DEBUG] User found: {user != null}");
        
        if (user == null || !user.IsActive)
        {
            Console.WriteLine($"[LoginAsync DEBUG] User not found or inactive");
            return (false, null, null);
        }

        Console.WriteLine($"[LoginAsync DEBUG] UserId: {user.UserId}, Username: {user.Username}, Role: {user.Role}");
        Console.WriteLine($"[LoginAsync DEBUG] Password length: {password.Length}");
        Console.WriteLine($"[LoginAsync DEBUG] Hash prefix: {user.PasswordHash.Substring(0, 20)}...");

        // Verify password first
        var verifyResult = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        Console.WriteLine($"[LoginAsync DEBUG] BCrypt.Verify result: {verifyResult}");
        
        if (!verifyResult)
        {
            Console.WriteLine($"[LoginAsync DEBUG] Password verification failed");
            return (false, null, null);
        }

        // Check role if specified (case-insensitive comparison)
        if (!string.IsNullOrEmpty(role))
        {
            var requestedRole = role.Trim();
            var userRole = user.Role?.Trim() ?? "";
            
            Console.WriteLine($"[LoginAsync DEBUG] Role check: requested='{requestedRole}', user.Role='{userRole}'");
            
            // Normalize roles for comparison (case-insensitive)
            var normalizedRequested = requestedRole.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" 
                                    : requestedRole.Equals("customer", StringComparison.OrdinalIgnoreCase) ? "Customer"
                                    : requestedRole;
            
            // Check if user role matches requested role (case-insensitive)
            if (!userRole.Equals(normalizedRequested, StringComparison.OrdinalIgnoreCase) && 
                !userRole.Equals(requestedRole, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[LoginAsync DEBUG] Role mismatch: required='{requestedRole}' (normalized: '{normalizedRequested}'), actual='{userRole}'");
                return (false, null, null);
            }
            
            // Additional role-based access control for admin
            // Only allow Admin role users to login with admin role request
            if (normalizedRequested == "Admin" && userRole != "Admin")
            {
                Console.WriteLine($"[LoginAsync DEBUG] Admin role required but user role is '{userRole}'");
                return (false, null, null);
            }
        }

        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        await _auditService.LogAsync("User", user.UserId, "Login", user.Email, 
            null, null, $"User {user.Email} logged in");

        return (true, token, user);
    }

    public async Task<(bool Success, string? Token, User? User)> CustomerLoginAsync(string emailOrUsername, string password)
    {
        // Tìm user theo email HOẶC username
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);
        
        if (user == null || !user.IsActive)
            return (false, null, null);

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, null, null);

        // Only allow customer role
        if (user.Role != "Customer")
            return (false, null, null);

        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        await _auditService.LogAsync("User", user.UserId, "CustomerLogin", user.Email,
            null, null, $"Customer {user.Email} logged in");

        return (true, token, user);
    }

    public async Task<User?> RegisterCustomerAsync(Customer customer, string username, string password)
    {
        // Check if email already exists
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == customer.Email);
        if (existingUser != null)
            return null;

        // Create customer first
        customer.CreatedAt = DateTime.UtcNow;
        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        // Create user account
        var user = new User
        {
            Username = username,
            Email = customer.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Customer",
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            CustomerId = customer.CustomerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("User", user.UserId, "Register", "System",
            null, null, $"New customer registered: {user.Email}");

        return user;
    }

    public string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = Encoding.UTF8.GetBytes(secretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("CustomerId", user.CustomerId?.ToString() ?? ""),
            new Claim("EmployeeId", user.EmployeeId?.ToString() ?? "")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(int.Parse(jwtSettings["ExpirationHours"] ?? "24")),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<User?> RegisterCustomerAsync(RegisterRequest request)
    {
        // Check if email already exists
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
            return null;

        // Create customer
        var customer = new Customer
        {
            FullName = $"{request.FirstName} {request.LastName}",
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            CustomerType = "Regular"
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        // Create user account
        var user = new User
        {
            Username = request.Email,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Customer",
            CustomerId = customer.CustomerId,
            FullName = $"{request.FirstName} {request.LastName}",
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }
}

