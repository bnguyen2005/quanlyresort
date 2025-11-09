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
}

