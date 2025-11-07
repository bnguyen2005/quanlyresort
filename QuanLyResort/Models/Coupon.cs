using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

public class Coupon
{
    [Key]
    public int CouponId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty; // Unique coupon code (e.g., SUMMER2024)

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(20)]
    public string Type { get; set; } = "percent"; // "percent" or "amount"

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; } // Percentage (1-100) or fixed amount

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxDiscount { get; set; } // Maximum discount amount (only for percentage type)

    public int? MaxUses { get; set; } // Maximum number of times coupon can be used (null = unlimited)

    public int UsesCount { get; set; } = 0; // Current number of times used

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [StringLength(100)]
    public string? UpdatedBy { get; set; }
}

