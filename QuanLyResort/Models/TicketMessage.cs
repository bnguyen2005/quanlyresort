using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

/// <summary>
/// Tin nhắn trong ticket (chat/conversation)
/// </summary>
public class TicketMessage
{
    [Key]
    public int MessageId { get; set; }

    [Required]
    public int TicketId { get; set; }
    public SupportTicket Ticket { get; set; } = null!;

    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string SenderType { get; set; } = "Customer"; // Customer, Staff, System

    [StringLength(100)]
    public string? SenderName { get; set; } // Tên người gửi

    [StringLength(255)]
    [EmailAddress]
    public string? SenderEmail { get; set; } // Email người gửi

    public bool IsInternal { get; set; } = false; // True nếu là ghi chú nội bộ (khách không thấy)

    public bool IsRead { get; set; } = false; // Đã đọc chưa

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // File đính kèm (nếu có)
    [StringLength(500)]
    public string? AttachmentUrl { get; set; }

    [StringLength(200)]
    public string? AttachmentName { get; set; }
}

