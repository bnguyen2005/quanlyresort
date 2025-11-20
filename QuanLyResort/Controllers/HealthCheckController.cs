using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;

namespace QuanLyResort.Controllers;

/// <summary>
/// Controller để kiểm tra health của service
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly ILogger<HealthCheckController> _logger;

    public HealthCheckController(
        ResortDbContext context,
        ILogger<HealthCheckController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint - Render sẽ gọi endpoint này để kiểm tra service có hoạt động không
    /// Public endpoint - không cần authentication
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            // Kiểm tra database connection
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                _logger.LogWarning("[Health Check] ⚠️ Database connection failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    database = "disconnected",
                    timestamp = DateTime.UtcNow
                });
            }

            // Test một query đơn giản để đảm bảo database thực sự hoạt động
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Health Check] ⚠️ Database query test failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    database = "query_failed",
                    timestamp = DateTime.UtcNow
                });
            }

            _logger.LogInformation("[Health Check] ✅ Service is healthy");
            return Ok(new
            {
                status = "healthy",
                database = "connected",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Health Check] ❌ Health check failed");
            return StatusCode(503, new
            {
                status = "unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}

