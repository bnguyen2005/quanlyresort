using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

public class AuditLog
{
    [Key]
    public int LogId { get; set; }

    [Required]
    [StringLength(50)]
    public string EntityName { get; set; } = string.Empty; // Booking, Room, Invoice, etc.

    [Required]
    public int EntityId { get; set; }

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, CheckIn, CheckOut, Payment, etc.

    [StringLength(100)]
    public string? PerformedBy { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(2000)]
    public string? OldValues { get; set; } // JSON

    [StringLength(2000)]
    public string? NewValues { get; set; } // JSON

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? IpAddress { get; set; }

    [StringLength(200)]
    public string? UserAgent { get; set; }
}

