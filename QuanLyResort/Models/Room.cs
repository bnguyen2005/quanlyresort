using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    [StringLength(20)]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string RoomType { get; set; } = string.Empty; // Standard, Deluxe, Suite, Villa

    // Foreign key to RoomType model
    public int? RoomTypeId { get; set; }

    [StringLength(50)]
    public string? Floor { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerNight { get; set; }

    public int MaxOccupancy { get; set; } = 2;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? Amenities { get; set; } // JSON string or comma-separated

    public bool IsAvailable { get; set; } = true;

    [Required]
    [StringLength(20)]
    public string HousekeepingStatus { get; set; } = "Clean"; // Clean, Dirty, InProgress, Ready

    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// URL hình ảnh của phòng (hình ảnh riêng của phòng này, khác với RoomType image)
    /// </summary>
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Danh sách hình ảnh gallery - JSON array of URLs (tối đa 5 hình)
    /// </summary>
    [StringLength(2000)]
    public string? ImageGallery { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public RoomType? RoomTypeNavigation { get; set; } // Link to RoomType model
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Charge> Charges { get; set; } = new List<Charge>();
}

