using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

public class Service
{
    [Key]
    public int ServiceId { get; set; }

    [Required]
    [StringLength(100)]
    public string ServiceName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ServiceType { get; set; } = string.Empty; // RoomService, Spa, Laundry, Transport, Restaurant, Activity

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [StringLength(20)]
    public string? Unit { get; set; } = "Unit"; // Unit, Hour, Person

    /// <summary>
    /// URL hình ảnh của dịch vụ/món ăn
    /// </summary>
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Charge> Charges { get; set; } = new List<Charge>();
}

