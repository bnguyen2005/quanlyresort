using Microsoft.AspNetCore.Mvc;
using QuanLyResort.Models;
using QuanLyResort.Services;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        Console.WriteLine($"[AuthController] Login request: Email={request.Email}, Role={request.Role}");
        
        // Sử dụng Email property làm emailOrUsername (có thể là email hoặc username)
        var (success, token, user) = await _authService.LoginAsync(request.Email, request.Password, request.Role);

        if (!success)
        {
            Console.WriteLine($"[AuthController] ❌ Login failed for: {request.Email}");
            return Unauthorized(new { message = "Invalid credentials or insufficient permissions" });
        }

        Console.WriteLine($"[AuthController] ✅ Login successful for: {request.Email}, Role: {user!.Role}");
        
        return Ok(new
        {
            token,
            email = user.Email,
            role = user.Role,
            user = new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.Role,
                user.FullName
            }
        });
    }

    [HttpPost("customer-login")]
    public async Task<IActionResult> CustomerLogin([FromBody] LoginRequest request)
    {
        // Sử dụng Email property làm emailOrUsername (có thể là email hoặc username)
        var (success, token, user) = await _authService.CustomerLoginAsync(request.Email, request.Password);

        if (!success)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(new
        {
            token,
            user = new
            {
                user!.UserId,
                user.Username,
                user.Email,
                user.Role,
                user.FullName,
                user.CustomerId
            }
        });
    }

    [HttpPost("register-customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegistrationRequest request)
    {
        var customer = new Customer
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PassportNumber = request.PassportNumber,
            Nationality = request.Nationality,
            CustomerType = "Regular"
        };

        var user = await _authService.RegisterCustomerAsync(customer, request.Username, request.Password);

        if (user == null)
            return BadRequest(new { message = "Email already exists" });

        return Ok(new { message = "Registration successful", userId = user.UserId });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

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

public class CustomerRegistrationRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PassportNumber { get; set; }
    public string? Nationality { get; set; }
}

