using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            await _dataSeeder.SeedAsync();
            return Ok(new { message = "Data seeded successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Seeding failed: {ex.Message}" });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetSystemStats()
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

