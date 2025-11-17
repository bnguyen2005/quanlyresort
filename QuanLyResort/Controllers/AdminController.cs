using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Services;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly DataSeeder _dataSeeder;

    public AdminController(ResortDbContext context)
    {
        _context = context;
        _dataSeeder = new DataSeeder(context);
    }

    [HttpPost("seed")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            Console.WriteLine("[AdminController] 🌱 Starting data seed...");
            await _dataSeeder.SeedAsync();
            Console.WriteLine("[AdminController] ✅ Data seeded successfully");
            
            // Return summary
            var userCount = _context.Users.Count();
            var adminCount = _context.Users.Count(u => u.Role == "Admin");
            
            return Ok(new { 
                message = "Data seeded successfully",
                usersCreated = userCount,
                adminUsers = adminCount,
                adminCredentials = new {
                    email = "admin@resort.test",
                    username = "admin",
                    password = "P@ssw0rd123"
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdminController] ❌ Seeding failed: {ex.Message}");
            Console.WriteLine($"[AdminController] Stack trace: {ex.StackTrace}");
            return BadRequest(new { message = $"Seeding failed: {ex.Message}", details = ex.ToString() });
        }
    }
    
    [HttpGet("check-users")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckUsers()
    {
        var users = await _context.Users.Select(u => new {
            u.UserId,
            u.Username,
            u.Email,
            u.Role,
            u.IsActive
        }).ToListAsync();
        
        return Ok(new {
            totalUsers = users.Count,
            users = users
        });
    }

    [HttpGet("stats")]
    public IActionResult GetSystemStats()
    {
        var stats = new
        {
            totalUsers = _context.Users.Count(),
            totalCustomers = _context.Customers.Count(),
            totalEmployees = _context.Employees.Count(),
            totalRooms = _context.Rooms.Count(),
            totalBookings = _context.Bookings.Count(),
            totalInvoices = _context.Invoices.Count(),
            pendingBookings = _context.Bookings.Count(b => b.Status == "Pending"),
            checkedInBookings = _context.Bookings.Count(b => b.Status == "CheckedIn"),
            availableRooms = _context.Rooms.Count(r => r.IsAvailable && r.HousekeepingStatus == "Ready")
        };

        return Ok(stats);
    }

    [HttpPost("backup")]
    public IActionResult BackupDatabase()
    {
        // TODO: Implement actual database backup logic
        return Ok(new { message = "Database backup initiated (placeholder)", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Reset password cho admin user hoặc tạo mới nếu chưa có
    /// </summary>
    [HttpPost("reset-admin-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetAdminPassword([FromBody] ResetPasswordRequest? request = null)
    {
        try
        {
            var newPassword = request?.Password ?? "P@ssw0rd123";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Tìm admin user
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == "admin@resort.test" || u.Username == "admin");

            if (adminUser == null)
            {
                // Tạo admin user mới nếu chưa có
                var adminEmployee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == "admin@resort.test");

                if (adminEmployee == null)
                {
                    // Tạo employee trước
                    adminEmployee = new Models.Employee
                    {
                        FullName = "Nguyễn Văn Admin",
                        Email = "admin@resort.test",
                        PhoneNumber = "0901234567",
                        Position = "Administrator",
                        Department = "Management",
                        IsActive = true
                    };
                    _context.Employees.Add(adminEmployee);
                    await _context.SaveChangesAsync();
                }

                adminUser = new Models.User
                {
                    Username = "admin",
                    Email = "admin@resort.test",
                    PasswordHash = passwordHash,
                    Role = "Admin",
                    FullName = "Nguyễn Văn Admin",
                    IsActive = true,
                    EmployeeId = adminEmployee.EmployeeId
                };
                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Admin user created successfully",
                    email = "admin@resort.test",
                    username = "admin",
                    password = newPassword
                });
            }
            else
            {
                // Reset password cho admin user hiện có
                adminUser.PasswordHash = passwordHash;
                adminUser.IsActive = true;
                _context.Users.Update(adminUser);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Admin password reset successfully",
                    email = adminUser.Email,
                    username = adminUser.Username,
                    password = newPassword
                });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Failed to reset admin password: {ex.Message}", details = ex.ToString() });
        }
    }

    /// <summary>
    /// Force create admin user (override existing)
    /// </summary>
    [HttpPost("force-create-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> ForceCreateAdmin()
    {
        try
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd123");

            // Đảm bảo có employee
            var adminEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == "admin@resort.test");

            if (adminEmployee == null)
            {
                adminEmployee = new Models.Employee
                {
                    FullName = "Nguyễn Văn Admin",
                    Email = "admin@resort.test",
                    PhoneNumber = "0901234567",
                    Position = "Administrator",
                    Department = "Management",
                    IsActive = true
                };
                _context.Employees.Add(adminEmployee);
                await _context.SaveChangesAsync();
            }

            // Xóa admin user cũ nếu có
            var existingAdmin = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == "admin@resort.test" || u.Username == "admin");
            
            if (existingAdmin != null)
            {
                _context.Users.Remove(existingAdmin);
                await _context.SaveChangesAsync();
            }

            // Tạo admin user mới
            var adminUser = new Models.User
            {
                Username = "admin",
                Email = "admin@resort.test",
                PasswordHash = passwordHash,
                Role = "Admin",
                FullName = "Nguyễn Văn Admin",
                IsActive = true,
                EmployeeId = adminEmployee.EmployeeId
            };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Admin user created/updated successfully",
                email = "admin@resort.test",
                username = "admin",
                password = "P@ssw0rd123"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Failed to create admin: {ex.Message}", details = ex.ToString() });
        }
    }
}

public class ResetPasswordRequest
{
    public string? Password { get; set; }
}

