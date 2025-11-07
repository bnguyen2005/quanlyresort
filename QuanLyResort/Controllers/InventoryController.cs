using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using System.Security.Claims;

namespace QuanLyResort.Controllers
{
[ApiController]
[Route("api/[controller]")]
    [Authorize]
public class InventoryController : ControllerBase
{
    private readonly ResortDbContext _context;

        public InventoryController(ResortDbContext context)
    {
        _context = context;
        }

        // GET: api/inventory/items
        [HttpGet("items")]
        public async Task<ActionResult<object>> GetInventoryItems(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] bool? lowStock = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                Console.WriteLine($"[Inventory] List items search='{search}', category='{category}', lowStock={lowStock}, page={page}, pageSize={pageSize}");
                var query = _context.InventoryItems.AsNoTracking()
                    .Where(i => i.IsActive)
                    .Include(i => i.Supplier)
                    .Include(i => i.Category)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(i => i.ItemName.Contains(search) || 
                                           i.ItemCode.Contains(search) ||
                                           i.Description.Contains(search));
                }

                if (!string.IsNullOrEmpty(category))
                {
                    if (int.TryParse(category, out var categoryId))
                    {
                        query = query.Where(i => i.CategoryId == categoryId);
                    }
                    else
                    {
                        query = query.Where(i => i.Category != null && i.Category.CategoryName == category);
                    }
                }

                if (lowStock.HasValue && lowStock.Value)
                {
                    query = query.Where(i => i.CurrentStock <= i.MinimumStock);
                }

                var totalItems = await query.CountAsync();
                Console.WriteLine($"[Inventory] totalItems after filters={totalItems}");
                
                // Show newest items first so vừa tạo sẽ xuất hiện ngay đầu danh sách
                var items = await query
                    .OrderByDescending(i => i.ItemId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(i => new
                    {
                        i.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.Description,
                        i.CurrentStock,
                        i.MinimumStock,
                        i.MaximumStock,
                        i.UnitPrice,
                        i.Unit,
                        i.Location,
                        i.IsActive,
                        i.LastUpdated,
                        Category = i.Category != null ? new { i.Category.CategoryId, i.Category.CategoryName } : null,
                        Supplier = i.Supplier != null ? new { i.Supplier.SupplierId, i.Supplier.SupplierName } : null,
                        StockStatus = i.CurrentStock <= i.MinimumStock ? "Low Stock" : 
                                    i.CurrentStock >= i.MaximumStock ? "Overstock" : "Normal",
                        StockValue = i.CurrentStock * i.UnitPrice
                    })
                    .ToListAsync();

                var result = new
                {
                    items,
                    totalItems,
                    currentPage = page,
                    totalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                    pageSize
                };
                Console.WriteLine($"[Inventory] returning items={items.Count} page={page}/{result.totalPages}");
                if (items.Count > 0)
                {
                    var preview = string.Join(", ", items.Take(5).Select(x => $"#{x.ItemId}:{x.ItemCode}"));
                    Console.WriteLine($"[Inventory] items preview: {preview}");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải danh sách hàng tồn kho", error = ex.Message });
            }
        }

        // GET: api/inventory/items/{id}
        [HttpGet("items/{id}")]
        public async Task<ActionResult<object>> GetInventoryItem(int id)
        {
            try
            {
                var item = await _context.InventoryItems
                    .Include(i => i.Supplier)
                    .Include(i => i.Category)
                    .Include(i => i.StockMovements)
                    .FirstOrDefaultAsync(i => i.ItemId == id);

                if (item == null)
                {
                    return NotFound(new { message = "Không tìm thấy sản phẩm" });
                }

                return Ok(new
                {
                    item.ItemId,
                    item.ItemCode,
                    item.ItemName,
                    item.Description,
                    item.CurrentStock,
                    item.MinimumStock,
                    item.MaximumStock,
                    item.UnitPrice,
                    item.Unit,
                    item.Location,
                    item.IsActive,
                    item.CreatedAt,
                    item.LastUpdated,
                    Category = item.Category != null ? new { item.Category.CategoryId, item.Category.CategoryName } : null,
                    Supplier = item.Supplier != null ? new { item.Supplier.SupplierId, item.Supplier.SupplierName, item.Supplier.ContactInfo } : null,
                    StockMovements = item.StockMovements?.OrderByDescending(sm => sm.MovementDate).Take(10).Select(sm => new
                    {
                        sm.MovementId,
                        sm.MovementType,
                        sm.Quantity,
                        sm.MovementDate,
                        sm.Reference,
                        sm.Notes,
                        sm.PerformedBy
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải thông tin sản phẩm", error = ex.Message });
            }
        }

        // POST: api/inventory/items
        [HttpPost("items")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<object>> CreateInventoryItem([FromBody] InventoryItemDto dto)
        {
            try
            {
                // Check if item code already exists
                var existingItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ItemCode == dto.ItemCode);

                if (existingItem != null)
                {
                    return BadRequest(new { message = "Mã sản phẩm đã tồn tại" });
                }

                var item = new InventoryItem
                {
                    ItemCode = dto.ItemCode,
                    ItemName = dto.ItemName,
                    Description = dto.Description,
                    CategoryId = dto.CategoryId,
                    SupplierId = dto.SupplierId,
                    CurrentStock = dto.InitialStock,
                    MinimumStock = dto.MinimumStock,
                    MaximumStock = dto.MaximumStock,
                    UnitPrice = dto.UnitPrice,
                    Unit = dto.Unit,
                    Location = dto.Location,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                Console.WriteLine($"[Inventory] Create item request: {dto.ItemCode} - {dto.ItemName}");
                _context.InventoryItems.Add(item);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[Inventory] Created itemId={item.ItemId}");

                // Create initial stock movement if there's initial stock
                if (dto.InitialStock > 0)
                {
                    var stockMovement = new StockMovement
                    {
                        ItemId = item.ItemId,
                        MovementType = "Initial Stock",
                        Quantity = dto.InitialStock,
                        MovementDate = DateTime.UtcNow,
                        Reference = "INIT-" + item.ItemCode,
                        Notes = "Initial stock entry",
                        PerformedBy = User.Identity?.Name ?? "System"
                    };

                    _context.StockMovements.Add(stockMovement);
                    await _context.SaveChangesAsync();
                }

                // Log audit
                await LogAuditAsync("InventoryItem", item.ItemId, "Create", null, 
                    System.Text.Json.JsonSerializer.Serialize(item));

                var newTotal = await _context.InventoryItems.CountAsync();
                return CreatedAtAction(nameof(GetInventoryItem), new { id = item.ItemId }, 
                    new { message = "Tạo sản phẩm thành công", itemId = item.ItemId, totalItems = newTotal });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo sản phẩm", error = ex.Message });
            }
        }

        // PUT: api/inventory/items/{id}
        [HttpPut("items/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<object>> UpdateInventoryItem(int id, [FromBody] InventoryItemDto dto)
        {
            try
            {
                Console.WriteLine($"[Inventory] Update item id={id}");
                var item = await _context.InventoryItems.FindAsync(id);
                if (item == null)
                {
                    return NotFound(new { message = "Không tìm thấy sản phẩm" });
                }

                var oldValues = System.Text.Json.JsonSerializer.Serialize(item);

                var oldCode = item.ItemCode;

                // Allow changing ItemCode with uniqueness validation
                if (!string.IsNullOrWhiteSpace(dto.ItemCode) && !string.Equals(dto.ItemCode, item.ItemCode, StringComparison.Ordinal))
                {
                    var duplicate = await _context.InventoryItems
                        .AsNoTracking()
                        .AnyAsync(i => i.ItemCode == dto.ItemCode && i.ItemId != id);
                    if (duplicate)
                    {
                        return BadRequest(new { message = "Mã sản phẩm đã tồn tại" });
                    }
                    item.ItemCode = dto.ItemCode.Trim();
                }

                // Update properties
                item.ItemName = dto.ItemName;
                item.Description = dto.Description;
                item.CategoryId = dto.CategoryId;
                item.SupplierId = dto.SupplierId;
                item.MinimumStock = dto.MinimumStock;
                item.MaximumStock = dto.MaximumStock;
                item.UnitPrice = dto.UnitPrice;
                item.Unit = dto.Unit;
                item.Location = dto.Location;
                item.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                Console.WriteLine($"[Inventory] Updated item id={id} code: {oldCode} -> {item.ItemCode}");

                // Log audit
                await LogAuditAsync("InventoryItem", item.ItemId, "Update", oldValues, 
                    System.Text.Json.JsonSerializer.Serialize(item));

                return Ok(new { message = "Cập nhật sản phẩm thành công", itemId = item.ItemId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật sản phẩm", error = ex.Message });
            }
        }

        // POST: api/inventory/items/{id}/stock-movement
        [HttpPost("items/{id}/stock-movement")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<object>> CreateStockMovement(int id, [FromBody] StockMovementDto dto)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                if (item == null)
                {
                    return NotFound(new { message = "Không tìm thấy sản phẩm" });
                }

                var oldStock = item.CurrentStock;

                // Calculate new stock based on movement type
                var newStock = dto.MovementType.ToLower() switch
                {
                    "in" or "purchase" or "adjustment_in" => oldStock + dto.Quantity,
                    "out" or "usage" or "sale" or "adjustment_out" => oldStock - dto.Quantity,
                    _ => oldStock
                };

                if (newStock < 0)
                {
                    return BadRequest(new { message = "Số lượng tồn kho không thể âm" });
                }

                // Update stock
                item.CurrentStock = newStock;
                item.LastUpdated = DateTime.UtcNow;

                // Create stock movement record
                var stockMovement = new StockMovement
                {
                    ItemId = id,
                    MovementType = dto.MovementType,
                    Quantity = dto.Quantity,
                    MovementDate = DateTime.UtcNow,
                    Reference = dto.Reference,
                    Notes = dto.Notes,
                    PerformedBy = User.Identity?.Name ?? "System"
                };

                _context.StockMovements.Add(stockMovement);
                await _context.SaveChangesAsync();

                // Log audit
                await LogAuditAsync("StockMovement", stockMovement.MovementId, "Create", null, 
                    System.Text.Json.JsonSerializer.Serialize(stockMovement));

                return Ok(new 
                { 
                    message = "Cập nhật tồn kho thành công",
                    oldStock,
                    newStock = item.CurrentStock,
                    movementId = stockMovement.MovementId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật tồn kho", error = ex.Message });
            }
        }

        // DELETE: api/inventory/items/{id}
        [HttpDelete("items/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<object>> SoftDeleteItem(int id)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                if (item == null)
                {
                    return NotFound(new { message = "Không tìm thấy sản phẩm" });
                }

                if (!item.IsActive)
                {
                    return Ok(new { message = "Sản phẩm đã ở trạng thái ẩn", itemId = id });
                }

                var oldValues = System.Text.Json.JsonSerializer.Serialize(item);
                item.IsActive = false;
                item.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await LogAuditAsync("InventoryItem", item.ItemId, "SoftDelete", oldValues,
                    System.Text.Json.JsonSerializer.Serialize(item));

                return Ok(new { message = "Đã ẩn sản phẩm", itemId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa sản phẩm", error = ex.Message });
            }
        }

        // PATCH: api/inventory/items/{id}/toggle-active
        [HttpPatch("items/{id}/toggle-active")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<object>> ToggleActive(int id)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                if (item == null)
                {
                    return NotFound(new { message = "Không tìm thấy sản phẩm" });
                }
                var oldValues = System.Text.Json.JsonSerializer.Serialize(item);
                item.IsActive = !item.IsActive;
                item.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await LogAuditAsync("InventoryItem", item.ItemId, "ToggleActive", oldValues,
                    System.Text.Json.JsonSerializer.Serialize(item));
                return Ok(new { message = "Đã cập nhật trạng thái", itemId = id, isActive = item.IsActive });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật trạng thái", error = ex.Message });
            }
        }

        // GET: api/inventory/categories
        [HttpGet("categories")]
        public async Task<ActionResult<object>> GetCategories()
        {
            try
            {
                var categories = await _context.InventoryCategories
                    .Where(c => c.IsActive)
                    .Select(c => new
                    {
                        c.CategoryId,
                        c.CategoryName,
                        c.Description,
                        ItemCount = _context.InventoryItems.Count(i => i.CategoryId == c.CategoryId && i.IsActive)
                    })
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải danh mục", error = ex.Message });
            }
        }

        // GET: api/inventory/suppliers
        [HttpGet("suppliers")]
        public async Task<ActionResult<object>> GetSuppliers()
        {
            try
            {
                var suppliers = await _context.Suppliers
                    .Where(s => s.IsActive)
                    .Select(s => new
                    {
                        s.SupplierId,
                        s.SupplierName,
                        s.ContactInfo,
                        s.Address,
                        ItemCount = _context.InventoryItems.Count(i => i.SupplierId == s.SupplierId && i.IsActive)
                    })
                    .OrderBy(s => s.SupplierName)
                    .ToListAsync();

                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải nhà cung cấp", error = ex.Message });
            }
        }

        // GET: api/inventory/dashboard-stats
        [HttpGet("dashboard-stats")]
        public async Task<ActionResult<object>> GetDashboardStats()
        {
            try
            {
                var totalItems = await _context.InventoryItems.CountAsync(i => i.IsActive);
                var lowStockItems = await _context.InventoryItems
                    .CountAsync(i => i.IsActive && i.CurrentStock <= i.MinimumStock);
                // SQLite không hỗ trợ Sum trên biểu thức decimal phức tạp -> chuyển sang client
                var totalValue = _context.InventoryItems
                    .Where(i => i.IsActive)
                    .Select(i => new { i.CurrentStock, i.UnitPrice })
                    .AsEnumerable()
                    .Sum(x => x.CurrentStock * x.UnitPrice);
                var totalSuppliers = await _context.Suppliers.CountAsync(s => s.IsActive);

                // Recent movements (last 7 days)
                var recentMovements = await _context.StockMovements
                    .Where(sm => sm.MovementDate >= DateTime.UtcNow.AddDays(-7))
                    .CountAsync();

                return Ok(new
                {
                    totalItems,
                    lowStockItems,
                    totalValue,
                    totalSuppliers,
                    recentMovements
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải thống kê", error = ex.Message });
            }
        }

        // GET: api/inventory/low-stock-alerts
        [HttpGet("low-stock-alerts")]
        public async Task<ActionResult<object>> GetLowStockAlerts()
        {
            try
            {
                var lowStockItems = await _context.InventoryItems
                    .Include(i => i.Category)
                    .Where(i => i.IsActive && i.CurrentStock <= i.MinimumStock)
                    .Select(i => new
                    {
                        i.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.CurrentStock,
                        i.MinimumStock,
                        i.Unit,
                        Category = i.Category != null ? i.Category.CategoryName : "Uncategorized",
                        StockDeficit = i.MinimumStock - i.CurrentStock,
                        AlertLevel = i.CurrentStock == 0 ? "Critical" : 
                                   i.CurrentStock <= (i.MinimumStock * 0.5) ? "High" : "Medium"
                    })
                    .OrderBy(i => i.CurrentStock)
                    .ToListAsync();

                return Ok(lowStockItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải cảnh báo tồn kho", error = ex.Message });
            }
        }

        private async Task LogAuditAsync(string entityName, int entityId, string action, string? oldValues, string newValues)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    OldValues = oldValues,
                    NewValues = newValues,
                    PerformedBy = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log audit failure but don't throw
                Console.WriteLine($"Audit log failed: {ex.Message}");
            }
        }
    }

    // DTOs
    public class InventoryItemDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? SupplierId { get; set; }
        public int InitialStock { get; set; }
        public int MinimumStock { get; set; }
        public int MaximumStock { get; set; }
        public decimal UnitPrice { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string? Location { get; set; }
    }

    public class StockMovementDto
    {
        public string MovementType { get; set; } = string.Empty; // "in", "out", "adjustment_in", "adjustment_out"
        public int Quantity { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
    }
}