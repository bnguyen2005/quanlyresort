using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

/// <summary>
/// Câu hỏi thường gặp (FAQ)
/// </summary>
public class FAQ
{
    [Key]
    public int FAQId { get; set; }

    [Required]
    [StringLength(200)]
    public string Question { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Answer { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = "General"; // General, Booking, Payment, Restaurant, Services, Other

    public int DisplayOrder { get; set; } = 0; // Thứ tự hiển thị

    public bool IsActive { get; set; } = true;

    public int ViewCount { get; set; } = 0; // Số lần xem

    public int HelpfulCount { get; set; } = 0; // Số người đánh giá hữu ích

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; } // Admin/Staff tạo FAQ
}

