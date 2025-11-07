using QuanLyResort.Models;

namespace QuanLyResort.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(string notificationType, string title, string message, 
        string? severity = "Info", string? targetRole = null, int? targetUserId = null,
        string? relatedEntity = null, int? relatedEntityId = null);
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string? role = null, int? userId = null);
    Task MarkAsReadAsync(int notificationId);
}

