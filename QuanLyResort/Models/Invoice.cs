using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

public class Invoice
{
    [Key]
    public int InvoiceId { get; set; }

    [Required]
    [StringLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty; // INV2025001

    [Required]
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    [Required]
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; } = 10.0m; // 10% VAT

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceDue { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Issued"; // Issued, Paid, PartiallyPaid, Cancelled

    public DateTime IssueDate { get; set; } = DateTime.UtcNow;

    public DateTime? PaidDate { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; } // Cash, CreditCard, BankTransfer, Momo, ZaloPay

    [StringLength(100)]
    public string? PaymentReference { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(100)]
    public string? IssuedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Charge> Charges { get; set; } = new List<Charge>();
}

