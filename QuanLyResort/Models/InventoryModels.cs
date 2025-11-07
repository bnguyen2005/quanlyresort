using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyResort.Models
{
    // Danh mục sản phẩm
    public class InventoryCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    }

    // Nhà cung cấp
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(200)]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ContactInfo { get; set; }

        [StringLength(1000)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }

    // Sản phẩm trong kho
    public class InventoryItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        [StringLength(50)]
        public string ItemCode { get; set; } = string.Empty; // Mã sản phẩm duy nhất

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int? CategoryId { get; set; }
        public int? SupplierId { get; set; }

        public int CurrentStock { get; set; } = 0;
        public int MinimumStock { get; set; } = 0;
        public int MaximumStock { get; set; } = 1000;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } = 0;

        [StringLength(20)]
        public string Unit { get; set; } = "pcs"; // đơn vị: pcs, kg, liter, etc.

        [StringLength(100)]
        public string? Location { get; set; } // Vị trí trong kho

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual InventoryCategory? Category { get; set; }
        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
    }

    // Lịch sử xuất nhập kho
    public class StockMovement
    {
        [Key]
        public int MovementId { get; set; }

        public int ItemId { get; set; }

        [Required]
        [StringLength(50)]
        public string MovementType { get; set; } = string.Empty; // "in", "out", "adjustment", "purchase", "usage"

        public int Quantity { get; set; }

        public DateTime MovementDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? Reference { get; set; } // Số chứng từ

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string PerformedBy { get; set; } = string.Empty;

        // Navigation properties
        public virtual InventoryItem Item { get; set; } = null!;
    }

    // Đơn đặt hàng
    public class PurchaseOrder
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        public int SupplierId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Delivered, Cancelled

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }

    // Chi tiết đơn đặt hàng
    public class PurchaseOrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }
        public int ItemId { get; set; }

        public int OrderedQuantity { get; set; }
        public int? ReceivedQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual PurchaseOrder Order { get; set; } = null!;
        public virtual InventoryItem Item { get; set; } = null!;
    }

    // Báo cáo tồn kho
    public class InventoryReport
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
        public string StockStatus { get; set; } = string.Empty;
        public DateTime LastMovementDate { get; set; }
    }
}
