using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuanLyResort.Models;

public class Booking
{
    [Key]
    public int BookingId { get; set; }

    [Required]
    [StringLength(50)]
    public string BookingCode { get; set; } = string.Empty; // Unique code like BKG2025001

    [Required]
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? RoomId { get; set; } // Null until room assigned
    public Room? Room { get; set; }

    [Required]
    [StringLength(50)]
    public string RequestedRoomType { get; set; } = string.Empty; // Standard, Deluxe, Suite, Villa

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    public int NumberOfGuests { get; set; } = 1;

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Assigned, CheckedIn, CheckedOut, Cancelled

    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedTotalAmount { get; set; }

    [StringLength(1000)]
    public string? SpecialRequests { get; set; }

    [StringLength(50)]
    public string? Source { get; set; } = "Direct"; // Direct, Online, Phone, WalkIn, Agent

    [StringLength(100)]
    public string? CreatedBy { get; set; } // Who created (Customer, FrontDesk staff)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ActualCheckInTime { get; set; }

    public DateTime? ActualCheckOutTime { get; set; }

    [StringLength(500)]
    public string? CancellationReason { get; set; }

    // Navigation properties
    public ICollection<Charge> Charges { get; set; } = new List<Charge>();
    [JsonIgnore]
    public Invoice? Invoice { get; set; }
}

