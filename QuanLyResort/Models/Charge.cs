using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

public class Charge
{
    [Key]
    public int ChargeId { get; set; }

    [Required]
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int? RoomId { get; set; }
    public Room? Room { get; set; }

    public int? ServiceId { get; set; }
    public Service? Service { get; set; }

    [Required]
    [StringLength(50)]
    public string ChargeType { get; set; } = string.Empty; // RoomCharge, ServiceCharge, Tax, Deposit, Other

    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(100)]
    public string? OutletName { get; set; } // Restaurant name, Spa name, etc.

    public DateTime ChargeDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Invoice? Invoice { get; set; }
}

