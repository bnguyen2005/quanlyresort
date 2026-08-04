using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

public class Review
{
    [Key]
    public int ReviewId { get; set; }

    [Required]
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? RoomId { get; set; } // Null if review is for general service
    public Room? Room { get; set; }

    public int? BookingId { get; set; } // Optional: link to booking
    public Booking? Booking { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; } // 1-5 stars

    [Required]
    [StringLength(500)]
    public string Comment { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Response { get; set; } // Admin response

    public DateTime? ResponseDate { get; set; }

    [StringLength(100)]
    public string? RespondedBy { get; set; } // Admin username

    public bool IsApproved { get; set; } = true; // For moderation

    public bool IsVisible { get; set; } = true; // Hide/show review

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

