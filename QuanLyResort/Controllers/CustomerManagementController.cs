using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Services;
using System.Security.Claims;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,FrontDesk,Business,Customer")]
public class CustomerManagementController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<CustomerManagementController> _logger;

    public CustomerManagementController(ResortDbContext context, IAuditService auditService, ILogger<CustomerManagementController> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả khách hàng
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCustomers(
        [FromQuery] string? customerType = null,
        [FromQuery] string? nationality = null,
        [FromQuery] string? search = null)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrEmpty(customerType))
            query = query.Where(c => c.CustomerType == customerType);

        if (!string.IsNullOrEmpty(nationality))
            query = query.Where(c => c.Nationality == nationality);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => 
                c.FullName.Contains(search) ||
                c.Email.Contains(search) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(search)) ||
                (c.PassportNumber != null && c.PassportNumber.Contains(search)) ||
                (c.IdCardNumber != null && c.IdCardNumber.Contains(search)));
        }

        var customers = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.CustomerId,
                c.FullName,
                c.Email,
                c.PhoneNumber,
                c.PassportNumber,
                c.IdCardNumber,
                c.Nationality,
                c.CustomerType,
                c.Address,
                c.DateOfBirth,
                c.TotalSpent,
                c.LoyaltyPoints,
                c.CreatedAt,
                c.UpdatedAt,
                TotalBookings = _context.Bookings.Count(b => b.CustomerId == c.CustomerId),
                ActiveBookings = _context.Bookings.Count(b => 
                    b.CustomerId == c.CustomerId && 
                    (b.Status == "Pending" || b.Status == "Confirmed" || b.Status == "CheckedIn"))
            })
            .ToListAsync();

        return Ok(customers);
    }

    /// <summary>
    /// Lấy thông tin khách hàng của chính mình (từ JWT token)
    /// Route này PHẢI đặt TRƯỚC route {id} để tránh conflict
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyCustomer()
    {
        try
        {
            _logger.LogInformation("[GetMyCustomer] Request received");
            
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userCustomerId = User.FindFirst("CustomerId")?.Value;
            
            _logger.LogInformation("[GetMyCustomer] UserEmail: {Email}, CustomerId from token: {CustomerId}", 
                userEmail ?? "null", userCustomerId ?? "null");
            
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("[GetMyCustomer] No user email found");
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
            }

            Customer? customer = null;

            // Thử lấy CustomerId từ token trước
            if (!string.IsNullOrEmpty(userCustomerId) && int.TryParse(userCustomerId, out int customerId))
            {
                _logger.LogInformation("[GetMyCustomer] Trying to find customer by CustomerId: {Id}", customerId);
                customer = await _context.Customers.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CustomerId == customerId);
                if (customer != null)
                {
                    _logger.LogInformation("[GetMyCustomer] Found customer by CustomerId: {Id}, Email: {Email}", 
                        customerId, customer.Email);
                }
            }

            // Nếu không tìm thấy qua CustomerId, thử tìm qua email
            if (customer == null)
            {
                _logger.LogInformation("[GetMyCustomer] Trying to find customer by email: {Email}", userEmail);
                customer = await _context.Customers.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Email != null && x.Email.ToLower() == userEmail.ToLower());
                if (customer != null)
                {
                    _logger.LogInformation("[GetMyCustomer] Found customer by email: {Email}, CustomerId: {Id}", 
                        userEmail, customer.CustomerId);
                }
            }

            // Nếu vẫn không tìm thấy, thử tìm User và lấy CustomerId từ đó
            if (customer == null)
            {
                _logger.LogInformation("[GetMyCustomer] Trying to find User by email: {Email}", userEmail);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == userEmail.ToLower());
                
                if (user == null)
                {
                    // Thử tìm với username
                    _logger.LogInformation("[GetMyCustomer] Trying to find User by username: {Email}", userEmail);
                    user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == userEmail.ToLower());
                }

                if (user != null)
                {
                    _logger.LogInformation("[GetMyCustomer] Found User: {Username}, CustomerId: {CustomerId}", 
                        user.Username, user.CustomerId);
                    
                    if (user.CustomerId.HasValue)
                    {
                        customer = await _context.Customers.AsNoTracking()
                            .FirstOrDefaultAsync(x => x.CustomerId == user.CustomerId.Value);
                        if (customer != null)
                        {
                            _logger.LogInformation("[GetMyCustomer] Found customer via User.CustomerId: {Id}", 
                                user.CustomerId.Value);
                        }
                    }
                }
            }

            if (customer == null)
            {
                _logger.LogWarning("[GetMyCustomer] Customer not found for email: {Email}", userEmail);
                return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
            }
            
            _logger.LogInformation("[GetMyCustomer] Successfully found customer: {Id}, {Name}, {Email}", 
                customer.CustomerId, customer.FullName, customer.Email);

            var bookings = await _context.Bookings.AsNoTracking()
                .Where(b => b.CustomerId == customer.CustomerId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.BookingId,
                    b.BookingCode,
                    BookingDate = b.CreatedAt,
                    b.CheckInDate,
                    b.CheckOutDate,
                    b.Status,
                    TotalAmount = b.EstimatedTotalAmount,
                    RoomNumber = b.Room != null ? b.Room.RoomNumber : null
                })
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                customer.CustomerId,
                customer.FullName,
                customer.Email,
                customer.PhoneNumber,
                customer.PassportNumber,
                customer.IdCardNumber,
                customer.Nationality,
                customer.CustomerType,
                customer.Address,
                customer.DateOfBirth,
                customer.TotalSpent,
                customer.LoyaltyPoints,
                customer.Notes,
                customer.CreatedAt,
                customer.UpdatedAt,
                Bookings = bookings
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load customer", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thông tin khách hàng theo ID
    /// Customer có thể xem thông tin của chính họ
    /// Route constraint: chỉ match số nguyên để tránh conflict với route "my"
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Manager,FrontDesk,Business,Customer")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        try
        {
            // Check if customer is trying to access their own data
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCustomerId = User.FindFirst("CustomerId")?.Value;
            
            if (userRole == "Customer" && userCustomerId != null && int.Parse(userCustomerId) != id)
            {
                return Forbid("Bạn chỉ có thể xem thông tin của chính mình");
            }
            
            var c = await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CustomerId == id);

            if (c == null)
                return NotFound(new { message = "Customer not found" });

            var bookings = await _context.Bookings.AsNoTracking()
                .Where(b => b.CustomerId == id)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.BookingId,
                    b.BookingCode,
                    BookingDate = b.CreatedAt,
                    b.CheckInDate,
                    b.CheckOutDate,
                    b.Status,
                    TotalAmount = b.EstimatedTotalAmount,
                    RoomNumber = b.Room != null ? b.Room.RoomNumber : null
                })
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                c.CustomerId,
                c.FullName,
                c.Email,
                c.PhoneNumber,
                c.PassportNumber,
                c.IdCardNumber,
                c.Nationality,
                c.CustomerType,
                c.Address,
                c.DateOfBirth,
                c.TotalSpent,
                c.LoyaltyPoints,
                c.Notes,
                c.CreatedAt,
                c.UpdatedAt,
                Bookings = bookings
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load customer", error = ex.Message });
        }
    }

    /// <summary>
    /// Tạo khách hàng mới
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        // Check if email exists
        if (await _context.Customers.AnyAsync(c => c.Email == request.Email))
            return BadRequest(new { message = "Email already exists" });

        var customer = new Customer
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PassportNumber = request.PassportNumber,
            IdCardNumber = request.IdCardNumber,
            Nationality = request.Nationality ?? "Vietnam",
            CustomerType = request.CustomerType ?? "Regular",
            Address = request.Address,
            DateOfBirth = request.DateOfBirth,
            TotalSpent = 0,
            LoyaltyPoints = 0,
            Notes = request.Notes
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "Customer",
            customer.CustomerId,
            "Create",
            User.Identity?.Name ?? "System",
            null,
            Newtonsoft.Json.JsonConvert.SerializeObject(customer),
            $"Created customer: {customer.FullName} ({customer.Email})"
        );

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customer);
    }

    /// <summary>
    /// Cập nhật thông tin khách hàng
    /// Customer có thể cập nhật thông tin của chính họ
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager,FrontDesk,Business,Customer")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
    {
        try
        {
            // Check if customer is trying to update their own data
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCustomerId = User.FindFirst("CustomerId")?.Value;
            
            if (userRole == "Customer" && userCustomerId != null && int.Parse(userCustomerId) != id)
            {
                return Forbid("Bạn chỉ có thể cập nhật thông tin của chính mình");
            }
            
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound(new { message = "Customer not found" });

            var oldValues = Newtonsoft.Json.JsonConvert.SerializeObject(customer);

            // Check if email changed and already exists
            if (request.Email != customer.Email &&
                await _context.Customers.AnyAsync(c => c.Email == request.Email && c.CustomerId != id))
                return BadRequest(new { message = "Email already exists" });

            customer.FullName = request.FullName;
            customer.Email = request.Email;
            customer.PhoneNumber = request.PhoneNumber;
            customer.PassportNumber = request.PassportNumber;
            customer.IdCardNumber = request.IdCardNumber;
            customer.Nationality = request.Nationality;
            customer.CustomerType = request.CustomerType;
            customer.Address = request.Address;
            customer.DateOfBirth = request.DateOfBirth;
            customer.Notes = request.Notes;
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log audit
            await _auditService.LogAsync(
                "Customer",
                customer.CustomerId,
                "Update",
                User.Identity?.Name ?? "System",
                oldValues,
                Newtonsoft.Json.JsonConvert.SerializeObject(customer),
                $"Updated customer: {customer.FullName}"
            );

            var result = new
            {
                customer.CustomerId,
                customer.FullName,
                customer.Email,
                customer.PhoneNumber,
                customer.PassportNumber,
                customer.IdCardNumber,
                customer.Nationality,
                customer.CustomerType,
                customer.Address,
                customer.DateOfBirth,
                customer.TotalSpent,
                customer.LoyaltyPoints,
                customer.Notes,
                customer.AvatarUrl,
                customer.CreatedAt,
                customer.UpdatedAt
            };

            return Ok(new { message = "Customer updated successfully", customer = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to update customer", error = ex.Message });
        }
    }

    /// <summary>
    /// Upload ảnh đại diện cho khách hàng
    /// POST /api/customermanagement/{id}/upload-avatar
    /// </summary>
    [HttpPost("{id:int}/upload-avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar(int id, [FromForm] IFormFile? file)
    {
        try
        {
            // Check if customer is trying to update their own data
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCustomerId = User.FindFirst("CustomerId")?.Value;
            
            if (userRole == "Customer" && userCustomerId != null && int.Parse(userCustomerId) != id)
            {
                return Forbid("Bạn chỉ có thể cập nhật ảnh đại diện của chính mình");
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound(new { message = "Customer not found" });

            var oldAvatarUrl = customer.AvatarUrl;

            // Nếu không có file, xóa avatar
            if (file == null || file.Length == 0)
            {
                customer.AvatarUrl = null;
                customer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(oldAvatarUrl) && oldAvatarUrl.StartsWith("/"))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldAvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try { System.IO.File.Delete(oldFilePath); } catch { }
                    }
                }

                await _auditService.LogAsync("Customer", id, "RemoveAvatar", User.Identity?.Name ?? "System", oldAvatarUrl, null);
                return Ok(new { message = "Avatar removed successfully.", avatarUrl = (string?)null });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed: JPG, JPEG, PNG, GIF, WEBP" });
            }

            // Validate file size (max 10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = "File size exceeds 10MB limit." });
            }

            // Tạo thư mục uploads nếu chưa có
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileName = $"avatar_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Xóa file cũ nếu có
            if (!string.IsNullOrEmpty(oldAvatarUrl) && oldAvatarUrl.StartsWith("/"))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldAvatarUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath) && oldFilePath != filePath)
                {
                    try { System.IO.File.Delete(oldFilePath); } catch { }
                }
            }

            // Update customer with new avatar URL
            var avatarUrl = $"/uploads/avatars/{fileName}";
            customer.AvatarUrl = avatarUrl;
            customer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Customer", id, "UploadAvatar", User.Identity?.Name ?? "System", oldAvatarUrl, avatarUrl);

            return Ok(new { message = "Avatar uploaded successfully.", avatarUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar for customer {CustomerId}", id);
            return StatusCode(500, new { message = "Failed to upload avatar", error = ex.Message });
        }
    }

    /// <summary>
    /// Thay đổi loại khách hàng (Regular, VIP, Corporate)
    /// </summary>
    [HttpPost("{id:int}/change-type")]
    [Authorize(Roles = "Admin,Manager,Business")]
    public async Task<IActionResult> ChangeCustomerType(int id, [FromBody] ChangeCustomerTypeRequest request)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        var oldType = customer.CustomerType;
        customer.CustomerType = request.NewType;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "Customer",
            customer.CustomerId,
            "ChangeType",
            User.Identity?.Name ?? "System",
            $"{{\"CustomerType\": \"{oldType}\"}}",
            $"{{\"CustomerType\": \"{request.NewType}\"}}",
            $"Changed customer type for {customer.FullName}: {oldType} → {request.NewType}"
        );

        return Ok(new { message = "Customer type changed successfully", customer });
    }

    /// <summary>
    /// Thêm loyalty points
    /// </summary>
    [HttpPost("{id:int}/add-points")]
    public async Task<IActionResult> AddLoyaltyPoints(int id, [FromBody] AddPointsRequest request)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        var oldPoints = customer.LoyaltyPoints;
        customer.LoyaltyPoints += request.Points;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "Customer",
            customer.CustomerId,
            "AddPoints",
            User.Identity?.Name ?? "System",
            $"{{\"LoyaltyPoints\": {oldPoints}}}",
            $"{{\"LoyaltyPoints\": {customer.LoyaltyPoints}}}",
            $"Added {request.Points} points to {customer.FullName}. Reason: {request.Reason}"
        );

        return Ok(new 
        { 
            message = "Loyalty points added successfully",
            loyaltyPoints = customer.LoyaltyPoints
        });
    }

    /// <summary>
    /// Xóa khách hàng
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        // Check if customer has active bookings
        var hasActiveBookings = await _context.Bookings.AnyAsync(b => 
            b.CustomerId == id && 
            (b.Status == "Pending" || b.Status == "Confirmed" || b.Status == "CheckedIn"));

        if (hasActiveBookings)
            return BadRequest(new { message = "Cannot delete customer with active bookings" });

        var customerInfo = $"{customer.FullName} ({customer.Email})";

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "Customer",
            id,
            "Delete",
            User.Identity?.Name ?? "System",
            null,
            null,
            $"Deleted customer: {customerInfo}"
        );

        return Ok(new { message = "Customer deleted successfully" });
    }

    /// <summary>
    /// Tìm kiếm khách hàng
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchCustomers([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Search query is required" });

        var customers = await _context.Customers
            .Where(c => 
                c.FullName.Contains(query) ||
                c.Email.Contains(query) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(query)) ||
                (c.PassportNumber != null && c.PassportNumber.Contains(query)) ||
                (c.IdCardNumber != null && c.IdCardNumber.Contains(query)))
            .Take(20)
            .Select(c => new
            {
                c.CustomerId,
                c.FullName,
                c.Email,
                c.PhoneNumber,
                c.CustomerType,
                c.LoyaltyPoints
            })
            .ToListAsync();

        return Ok(customers);
    }

    /// <summary>
    /// Lấy danh sách loại khách hàng
    /// </summary>
    [HttpGet("types")]
    public IActionResult GetCustomerTypes()
    {
        var types = new[]
        {
            new { value = "Regular", label = "Thường", description = "Khách hàng thông thường" },
            new { value = "VIP", label = "VIP", description = "Khách hàng quan trọng" },
            new { value = "Corporate", label = "Doanh nghiệp", description = "Khách hàng công ty" },
            new { value = "Member", label = "Thành viên", description = "Thành viên thường xuyên" }
        };

        return Ok(types);
    }

    /// <summary>
    /// Thống kê khách hàng
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var totalCustomers = await _context.Customers.CountAsync();

            var byType = await _context.Customers
                .GroupBy(c => c.CustomerType)
                .Select(g => new { type = g.Key, count = g.Count() })
                .ToListAsync();

            var byNationality = await _context.Customers
                .GroupBy(c => c.Nationality)
                .Select(g => new { nationality = g.Key ?? "Unknown", count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToListAsync();

            // SQLite không order trực tiếp tốt với decimal => chuyển sang client để sắp xếp an toàn
            var topSpenders = _context.Customers
                .Select(c => new { c.CustomerId, c.FullName, c.TotalSpent, c.LoyaltyPoints })
                .AsEnumerable()
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToList();

            var recentCustomers = await _context.Customers
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .Select(c => new { c.CustomerId, c.FullName, c.Email, c.CreatedAt })
                .ToListAsync();

            return Ok(new
            {
                totalCustomers,
                byType,
                byNationality,
                topSpenders,
                recentCustomers
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load customer statistics", error = ex.Message });
        }
    }
}

// DTOs
public class CreateCustomerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PassportNumber { get; set; }
    public string? IdCardNumber { get; set; }
    public string? Nationality { get; set; }
    public string? CustomerType { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCustomerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PassportNumber { get; set; }
    public string? IdCardNumber { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
}

public class ChangeCustomerTypeRequest
{
    public string NewType { get; set; } = string.Empty;
}

public class AddPointsRequest
{
    public int Points { get; set; }
    public string? Reason { get; set; }
}

