using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

/// <summary>
/// Đơn đặt món nhà hàng
/// </summary>
public class RestaurantOrder
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    [StringLength(50)]
    public string OrderNumber { get; set; } = string.Empty; // ORD2025001

    public int? CustomerId { get; set; } // Null if walk-in guest
    public Customer? Customer { get; set; }

    public int? BookingId { get; set; } // Null if not linked to booking
    public Booking? Booking { get; set; }

    [Required]
    [StringLength(30)]
    [RegularExpression("^(Pending|Confirmed|Preparing|Ready|Delivered|Cancelled)$", ErrorMessage = "Status must be one of: Pending, Confirmed, Preparing, Ready, Delivered, Cancelled")]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Preparing, Ready, Delivered, Cancelled

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "TotalAmount must be greater than or equal to 0")]
    public decimal TotalAmount { get; set; }

    [StringLength(500)]
    public string? DeliveryAddress { get; set; } // Room number or address

    public DateTime? RequestedDeliveryTime { get; set; }

    [StringLength(500)]
    public string? SpecialRequests { get; set; }

    [StringLength(50)]
    [RegularExpression("^(Cash|Card|QR|RoomCharge|BankTransfer)$", ErrorMessage = "PaymentMethod must be one of: Cash, Card, QR, RoomCharge, BankTransfer")]
    public string? PaymentMethod { get; set; } // Cash, Card, QR, RoomCharge, BankTransfer

    [Required]
    [StringLength(30)]
    [RegularExpression("^(Unpaid|Paid|Refunded)$", ErrorMessage = "PaymentStatus must be one of: Unpaid, Paid, Refunded")]
    public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid, Refunded

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<RestaurantOrderItem> OrderItems { get; set; } = new List<RestaurantOrderItem>();
}

/// <summary>
/// Chi tiết đơn đặt món
/// </summary>
public class RestaurantOrderItem
{
    [Key]
    public int OrderItemId { get; set; }

    [Required]
    public int OrderId { get; set; }
    public RestaurantOrder Order { get; set; } = null!;

    [Required]
    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "UnitPrice must be greater than or equal to 0")]
    public decimal UnitPrice { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "SubTotal must be greater than or equal to 0")]
    public decimal SubTotal { get; set; }

    [StringLength(200)]
    public string? SpecialNote { get; set; } // Ghi chú cho món này
}
