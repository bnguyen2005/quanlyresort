using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models;

/// <summary>
/// Loại phòng - Room Type
/// VD: Standard, Deluxe, Suite, Villa
/// </summary>
public class RoomType
{
    [Key]
    public int RoomTypeId { get; set; }

    [Required]
    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty; // Standard, Deluxe, Suite, Villa, Bungalow

    [Required]
    [StringLength(50)]
    public string TypeCode { get; set; } = string.Empty; // STD, DLX, SUT, VIL (unique code)

    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Giá cơ bản mỗi đêm
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Sức chứa tối đa (số người)
    /// </summary>
    [Required]
    public int MaxOccupancy { get; set; } = 2;

    /// <summary>
    /// Số người tiêu chuẩn (không tính phụ phí)
    /// </summary>
    public int StandardOccupancy { get; set; } = 2;

    /// <summary>
    /// Phụ phí cho mỗi người thêm (ngoài StandardOccupancy)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ExtraPersonCharge { get; set; }

    /// <summary>
    /// Diện tích phòng (m²)
    /// </summary>
    public int? RoomSize { get; set; }

    /// <summary>
    /// Loại giường - VD: 1 King Bed, 2 Queen Beds
    /// </summary>
    [StringLength(100)]
    public string? BedType { get; set; }

    /// <summary>
    /// Danh sách tiện nghi - JSON hoặc comma-separated
    /// VD: WiFi, TV, Air Conditioning, Minibar, Balcony, Ocean View
    /// </summary>
    [StringLength(2000)]
    public string? Amenities { get; set; }

    /// <summary>
    /// Hình ảnh chính
    /// </summary>
    [StringLength(500)]
    public string? MainImageUrl { get; set; }

    /// <summary>
    /// Danh sách hình ảnh - JSON array of URLs
    /// </summary>
    [StringLength(2000)]
    public string? ImageGallery { get; set; }

    /// <summary>
    /// Trạng thái hoạt động
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Thứ tự hiển thị (để sắp xếp khi show cho khách)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}

