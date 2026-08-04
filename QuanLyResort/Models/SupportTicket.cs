using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

/// <summary>
/// Ticket hỗ trợ khách hàng
/// </summary>
public class SupportTicket
{
    [Key]
    public int TicketId { get; set; }

    [Required]
    [StringLength(50)]
    public string TicketNumber { get; set; } = string.Empty; // TKT2025001

    public int? CustomerId { get; set; } // Null nếu là khách vãng lai
    public Customer? Customer { get; set; }

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = "General"; // General, Booking, Payment, Restaurant, Services, Complaint, Other

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Open"; // Open, InProgress, WaitingCustomer, Resolved, Closed, Cancelled

    [Required]
    [StringLength(30)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

    [StringLength(100)]
    public string? AssignedTo { get; set; } // Email/Username của nhân viên được giao

    [StringLength(100)]
    public string? CreatedBy { get; set; } // Email/Username của người tạo

    // Thông tin liên hệ (cho khách vãng lai)
    [StringLength(100)]
    public string? ContactName { get; set; }

    [StringLength(255)]
    [EmailAddress]
    public string? ContactEmail { get; set; }

    [StringLength(20)]
    public string? ContactPhone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    [StringLength(1000)]
    public string? ResolutionNotes { get; set; } // Ghi chú giải quyết

    // Navigation properties
    public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
}

