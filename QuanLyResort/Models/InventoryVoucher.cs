using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

public class InventoryVoucher
{
    [Key]
    public int VoucherId { get; set; }

    [Required]
    [StringLength(50)]
    public string VoucherNumber { get; set; } = string.Empty; // VOUCHER2025001

    [Required]
    [StringLength(30)]
    public string VoucherType { get; set; } = string.Empty; // Purchase, Consumption, Return, Adjustment

    [Required]
    [StringLength(100)]
    public string ItemName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? ItemCode { get; set; }

    [StringLength(50)]
    public string? Category { get; set; } // Linen, Toiletries, Food, Beverage, Cleaning

    public int Quantity { get; set; }

    [StringLength(20)]
    public string? Unit { get; set; } = "Unit";

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(200)]
    public string? Supplier { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }

    public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(30)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
}

