using QuanLyResort.Models;
using QuanLyResort.Repositories;

namespace QuanLyResort.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateNotificationAsync(string notificationType, string title, string message,
        string? severity = "Info", string? targetRole = null, int? targetUserId = null,
        string? relatedEntity = null, int? relatedEntityId = null)
    {
        var notification = new Notification
        {
            NotificationType = notificationType,
            Title = title,
            Message = message,
            Severity = severity,
            TargetRole = targetRole,
            TargetUserId = targetUserId,
            RelatedEntity = relatedEntity,
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string? role = null, int? userId = null)
    {
        var notifications = await _unitOfWork.Notifications.FindAsync(n => !n.IsRead);

        if (!string.IsNullOrEmpty(role))
        {
            notifications = notifications.Where(n => n.TargetRole == role || n.TargetRole == null);
        }

        if (userId.HasValue)
        {
            notifications = notifications.Where(n => n.TargetUserId == userId || n.TargetUserId == null);
        }

        return notifications.OrderByDescending(n => n.CreatedAt);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

