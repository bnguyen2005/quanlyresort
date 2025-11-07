using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Services;
using BCrypt.Net;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UserManagementController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly IAuditService _auditService;

    public UserManagementController(ResortDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    /// <summary>
    /// Lấy danh sách tất cả users
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? role = null, [FromQuery] bool? isActive = null)
    {
        var query = _context.Users
            .Include(u => u.Customer)
            .Include(u => u.Employee)
            .AsQueryable();

        // Filter by role
        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => u.Role == role);
        }

        // Filter by active status
        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new
            {
                u.UserId,
                u.Username,
                u.Email,
                u.Role,
                u.FullName,
                u.PhoneNumber,
                u.IsActive,
                u.CreatedAt,
                u.LastLoginAt,
                u.CustomerId,
                u.EmployeeId,
                CustomerName = u.Customer != null ? u.Customer.FullName : null,
                EmployeeName = u.Employee != null ? u.Employee.FullName : null,
                EmployeePosition = u.Employee != null ? u.Employee.Position : null
            })
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Lấy thông tin user theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Customer)
            .Include(u => u.Employee)
            .Where(u => u.UserId == id)
            .Select(u => new
            {
                u.UserId,
                u.Username,
                u.Email,
                u.Role,
                u.FullName,
                u.PhoneNumber,
                u.IsActive,
                u.CreatedAt,
                u.LastLoginAt,
                u.CustomerId,
                u.EmployeeId,
                Customer = u.Customer,
                Employee = u.Employee
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(user);
    }

    /// <summary>
    /// Tạo user mới
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Check if username exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest(new { message = "Username already exists" });

        // Check if email exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "Email already exists" });

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            IsActive = request.IsActive ?? true,
            EmployeeId = request.EmployeeId,
            CustomerId = request.CustomerId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "User",
            user.UserId,
            "Create",
            User.Identity?.Name ?? "System",
            null,
            Newtonsoft.Json.JsonConvert.SerializeObject(user),
            $"Created user: {user.Username} ({user.Role})"
        );

        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, new
        {
            user.UserId,
            user.Username,
            user.Email,
            user.Role,
            user.FullName
        });
    }

    /// <summary>
    /// Cập nhật thông tin user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var oldValues = Newtonsoft.Json.JsonConvert.SerializeObject(user);

        // Check if username changed and already exists
        if (request.Username != user.Username && 
            await _context.Users.AnyAsync(u => u.Username == request.Username && u.UserId != id))
            return BadRequest(new { message = "Username already exists" });

        // Check if email changed and already exists
        if (request.Email != user.Email && 
            await _context.Users.AnyAsync(u => u.Email == request.Email && u.UserId != id))
            return BadRequest(new { message = "Email already exists" });

        user.Username = request.Username;
        user.Email = request.Email;
        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.Role = request.Role;
        
        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "User",
            user.UserId,
            "Update",
            User.Identity?.Name ?? "System",
            oldValues,
            Newtonsoft.Json.JsonConvert.SerializeObject(user),
            $"Updated user: {user.Username}"
        );

        return Ok(new { message = "User updated successfully", user });
    }

    /// <summary>
    /// Đổi mật khẩu user
    /// </summary>
    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        
        // Debug: Verify immediately after hashing
        var verifyResult = BCrypt.Net.BCrypt.Verify(request.NewPassword, newHash);
        
        Console.WriteLine($"[ChangePassword DEBUG] UserId: {id}");
        Console.WriteLine($"[ChangePassword DEBUG] New password length: {request.NewPassword.Length}");
        Console.WriteLine($"[ChangePassword DEBUG] New hash: {newHash.Substring(0, 20)}...");
        Console.WriteLine($"[ChangePassword DEBUG] Immediate verify: {verifyResult}");
        
        user.PasswordHash = newHash;
        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "User",
            user.UserId,
            "ChangePassword",
            User.Identity?.Name ?? "System",
            null,
            null,
            $"Changed password for user: {user.Username}"
        );

        return Ok(new { 
            message = "Password changed successfully",
            debug = new {
                userId = id,
                username = user.Username,
                immediateVerify = verifyResult,
                hashPrefix = newHash.Substring(0, 20)
            }
        });
    }

    /// <summary>
    /// Đổi role của user
    /// </summary>
    [HttpPost("{id}/change-role")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var oldRole = user.Role;
        user.Role = request.NewRole;
        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "User",
            user.UserId,
            "ChangeRole",
            User.Identity?.Name ?? "System",
            $"{{\"Role\": \"{oldRole}\"}}",
            $"{{\"Role\": \"{request.NewRole}\"}}",
            $"Changed role for user {user.Username}: {oldRole} → {request.NewRole}"
        );

        return Ok(new { message = "Role changed successfully", user });
    }

    /// <summary>
    /// Khóa/Mở khóa user
    /// </summary>
    [HttpPost("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "User",
            user.UserId,
            user.IsActive ? "Activate" : "Deactivate",
            User.Identity?.Name ?? "System",
            null,
            null,
            $"{(user.IsActive ? "Activated" : "Deactivated")} user: {user.Username}"
        );

        return Ok(new 
        { 
            message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully",
            isActive = user.IsActive
        });
    }

    /// <summary>
    /// Xóa user (soft delete - chỉ deactivate)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        // Soft delete - just deactivate
        user.IsActive = false;
        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "User",
            user.UserId,
            "Delete",
            User.Identity?.Name ?? "System",
            null,
            null,
            $"Deleted (deactivated) user: {user.Username}"
        );

        return Ok(new { message = "User deleted successfully" });
    }

    /// <summary>
    /// Xóa user vĩnh viễn (hard delete)
    /// </summary>
    [HttpDelete("{id}/permanent")]
    public async Task<IActionResult> PermanentDeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var userInfo = $"{user.Username} ({user.Email})";

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAsync(
            "User",
            id,
            "PermanentDelete",
            User.Identity?.Name ?? "System",
            null,
            null,
            $"Permanently deleted user: {userInfo}"
        );

        return Ok(new { message = "User permanently deleted" });
    }

    /// <summary>
    /// Lấy danh sách roles có sẵn
    /// </summary>
    [HttpGet("roles")]
    public IActionResult GetRoles()
    {
        var roles = new[]
        {
            new { value = "Admin", label = "Quản trị viên", description = "Quyền cao nhất, quản lý toàn bộ hệ thống" },
            new { value = "Manager", label = "Quản lý", description = "Quản lý resort, xem báo cáo" },
            new { value = "Business", label = "Kinh doanh", description = "Quản lý đặt phòng, khách hàng" },
            new { value = "FrontDesk", label = "Lễ tân", description = "Check-in, check-out, đặt phòng" },
            new { value = "Cashier", label = "Thu ngân", description = "Thanh toán, hóa đơn" },
            new { value = "Accounting", label = "Kế toán", description = "Báo cáo tài chính" },
            new { value = "Inventory", label = "Kho", description = "Quản lý kho, nhập xuất" },
            new { value = "Housekeeping", label = "Dọn phòng", description = "Quản lý dọn dẹp phòng" },
            new { value = "Maintenance", label = "Kỹ thuật", description = "Sửa chữa, bảo trì" },
            new { value = "Customer", label = "Khách hàng", description = "Khách hàng sử dụng dịch vụ" }
        };

        return Ok(roles);
    }
}

// DTOs
public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
    public int? EmployeeId { get; set; }
    public int? CustomerId { get; set; }
}

public class UpdateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
}

public class ChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangeRoleRequest
{
    public string NewRole { get; set; } = string.Empty;
}

