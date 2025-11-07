using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [Required]
    [StringLength(50)]
    public string NotificationType { get; set; } = string.Empty; // Alert, Warning, Info, Success

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Severity { get; set; } = "Info"; // Low, Medium, High, Critical

    [StringLength(100)]
    public string? TargetRole { get; set; } // Admin, FrontDesk, Manager, etc. (null = all)

    public int? TargetUserId { get; set; }

    [StringLength(50)]
    public string? RelatedEntity { get; set; } // Booking, Room, Invoice, etc.

    public int? RelatedEntityId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    [StringLength(500)]
    public string? ActionUrl { get; set; }
}

