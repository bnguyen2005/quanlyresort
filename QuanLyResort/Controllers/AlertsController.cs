using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyResort.Services;
using System.Security.Claims;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public AlertsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAlerts()
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var notifications = await _notificationService.GetUnreadNotificationsAsync(userRole, userId);
        return Ok(notifications);
    }

    [HttpPost("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(new { message = "Notification marked as read" });
    }

    [HttpPost("generate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateTestAlert([FromBody] GenerateAlertRequest request)
    {
        await _notificationService.CreateNotificationAsync(
            request.NotificationType,
            request.Title,
            request.Message,
            request.Severity,
            request.TargetRole
        );

        return Ok(new { message = "Alert generated successfully" });
    }
}

public class GenerateAlertRequest
{
    public string NotificationType { get; set; } = "Info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
    public string? TargetRole { get; set; }
}

