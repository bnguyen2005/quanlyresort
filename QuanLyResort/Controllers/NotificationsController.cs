using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Services;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        ResortDbContext context,
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    // GET: api/notifications
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var userRole = User.FindFirst("Role")?.Value;

            IQueryable<Notification> query = _context.Notifications;

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            // Filter by user or role
            query = query.Where(n => 
                (n.TargetUserId == userId || n.TargetUserId == null) &&
                (n.TargetRole == userRole || n.TargetRole == null)
            );

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return StatusCode(500, new { message = "Lỗi khi lấy thông báo", error = ex.Message });
        }
    }

    // GET: api/notifications/unread-count
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var userRole = User.FindFirst("Role")?.Value;

            var count = await _context.Notifications
                .Where(n => !n.IsRead &&
                           (n.TargetUserId == userId || n.TargetUserId == null) &&
                           (n.TargetRole == userRole || n.TargetRole == null))
                .CountAsync();

            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return StatusCode(500, new { message = "Lỗi khi lấy số thông báo chưa đọc", error = ex.Message });
        }
    }

    // PATCH: api/notifications/{id}/read
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { message = "Đã đánh dấu đã đọc" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return StatusCode(500, new { message = "Lỗi khi đánh dấu đã đọc", error = ex.Message });
        }
    }

    // PATCH: api/notifications/read-all
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var userRole = User.FindFirst("Role")?.Value;

            var notifications = await _context.Notifications
                .Where(n => !n.IsRead &&
                           (n.TargetUserId == userId || n.TargetUserId == null) &&
                           (n.TargetRole == userRole || n.TargetRole == null))
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã đánh dấu tất cả đã đọc", count = notifications.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return StatusCode(500, new { message = "Lỗi khi đánh dấu tất cả đã đọc", error = ex.Message });
        }
    }

    // DELETE: api/notifications/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound(new { message = "Thông báo không tồn tại" });
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa thông báo" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification");
            return StatusCode(500, new { message = "Lỗi khi xóa thông báo", error = ex.Message });
        }
    }
}

