using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Services;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/room-types")]
[Authorize] // Yêu cầu authentication
public class RoomTypesController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<RoomTypesController> _logger;

    public RoomTypesController(
        ResortDbContext context,
        IAuditService auditService,
        ILogger<RoomTypesController> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả loại phòng
    /// GET /api/room-types
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // Khách vãng lai cũng có thể xem
    public async Task<IActionResult> GetAllRoomTypes([FromQuery] bool includeInactive = false)
    {
        var query = _context.RoomTypes.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(rt => rt.IsActive);
        }

        var roomTypes = await query
            .OrderBy(rt => rt.DisplayOrder)
            .ThenBy(rt => rt.TypeName)
            .ToListAsync();

        return Ok(roomTypes);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một loại phòng
    /// GET /api/room-types/{id}
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoomTypeById(int id)
    {
        try
        {
            _logger.LogInformation($"[RoomTypes] Getting room type by ID: {id}");
            
            var roomType = await _context.RoomTypes
                .Include(rt => rt.Rooms) // Include rooms để xem có bao nhiêu phòng thuộc loại này
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

            if (roomType == null)
            {
                _logger.LogWarning($"[RoomTypes] Room type {id} not found");
                return NotFound(new { message = "Không tìm thấy loại phòng" });
            }

            _logger.LogInformation($"[RoomTypes] Found room type: {roomType.TypeName}, Rooms count: {roomType.Rooms?.Count ?? 0}");

            // Thêm thống kê số phòng
            var stats = new
            {
                totalRooms = roomType.Rooms?.Count ?? 0,
                availableRooms = roomType.Rooms?.Count(r => r.IsAvailable) ?? 0
            };

            _logger.LogInformation($"[RoomTypes] Returning stats: Total={stats.totalRooms}, Available={stats.availableRooms}");

            // Tạo response object để tránh circular reference
            var response = new
            {
                roomType = new
                {
                    roomType.RoomTypeId,
                    roomType.TypeName,
                    roomType.TypeCode,
                    roomType.Description,
                    roomType.BasePrice,
                    roomType.MaxOccupancy,
                    roomType.StandardOccupancy,
                    roomType.ExtraPersonCharge,
                    roomType.RoomSize,
                    roomType.BedType,
                    roomType.Amenities,
                    roomType.IsActive,
                    roomType.DisplayOrder,
                    roomType.MainImageUrl,
                    roomType.ImageGallery,
                    roomType.CreatedAt,
                    roomType.UpdatedAt
                },
                stats
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[RoomTypes] Error getting room type {id}");
            return StatusCode(500, new { message = "Lỗi server khi lấy thông tin loại phòng", error = ex.Message });
        }
    }

    /// <summary>
    /// Tạo loại phòng mới
    /// POST /api/room-types
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateRoomType([FromBody] RoomType roomType)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Kiểm tra TypeCode đã tồn tại chưa
        var existingCode = await _context.RoomTypes
            .AnyAsync(rt => rt.TypeCode.ToLower() == roomType.TypeCode.ToLower());

        if (existingCode)
        {
            return BadRequest(new { message = $"Mã loại phòng '{roomType.TypeCode}' đã tồn tại" });
        }

        roomType.CreatedAt = DateTime.UtcNow;
        roomType.UpdatedAt = null;

        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();

        // Audit log
        var username = User.Identity?.Name ?? "System";
        await _auditService.LogAsync(
            "RoomType",
            roomType.RoomTypeId,
            "Create",
            username,
            null,
            Newtonsoft.Json.JsonConvert.SerializeObject(roomType),
            $"Tạo loại phòng mới: {roomType.TypeName}"
        );

        _logger.LogInformation($"[RoomTypes] Created new room type: {roomType.TypeName} by {username}");

        return CreatedAtAction(
            nameof(GetRoomTypeById),
            new { id = roomType.RoomTypeId },
            roomType
        );
    }

    /// <summary>
    /// Cập nhật loại phòng
    /// PUT /api/room-types/{id}
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateRoomType(int id, [FromBody] RoomType updatedRoomType)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var roomType = await _context.RoomTypes.FindAsync(id);
        if (roomType == null)
        {
            return NotFound(new { message = "Không tìm thấy loại phòng" });
        }

        // Kiểm tra TypeCode trùng (ngoại trừ chính nó)
        var duplicateCode = await _context.RoomTypes
            .AnyAsync(rt => rt.TypeCode.ToLower() == updatedRoomType.TypeCode.ToLower() 
                         && rt.RoomTypeId != id);

        if (duplicateCode)
        {
            return BadRequest(new { message = $"Mã loại phòng '{updatedRoomType.TypeCode}' đã tồn tại" });
        }

        // Lưu old values cho audit
        var oldValues = Newtonsoft.Json.JsonConvert.SerializeObject(roomType);

        // Update properties
        roomType.TypeName = updatedRoomType.TypeName;
        roomType.TypeCode = updatedRoomType.TypeCode;
        roomType.Description = updatedRoomType.Description;
        roomType.BasePrice = updatedRoomType.BasePrice;
        roomType.MaxOccupancy = updatedRoomType.MaxOccupancy;
        roomType.StandardOccupancy = updatedRoomType.StandardOccupancy;
        roomType.ExtraPersonCharge = updatedRoomType.ExtraPersonCharge;
        roomType.RoomSize = updatedRoomType.RoomSize;
        roomType.BedType = updatedRoomType.BedType;
        roomType.Amenities = updatedRoomType.Amenities;
        roomType.MainImageUrl = updatedRoomType.MainImageUrl;
        roomType.ImageGallery = updatedRoomType.ImageGallery;
        roomType.IsActive = updatedRoomType.IsActive;
        roomType.DisplayOrder = updatedRoomType.DisplayOrder;
        roomType.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Audit log
        var username = User.Identity?.Name ?? "System";
        await _auditService.LogAsync(
            "RoomType",
            roomType.RoomTypeId,
            "Update",
            username,
            oldValues,
            Newtonsoft.Json.JsonConvert.SerializeObject(roomType),
            $"Cập nhật loại phòng: {roomType.TypeName}"
        );

        _logger.LogInformation($"[RoomTypes] Updated room type: {roomType.TypeName} by {username}");

        return Ok(roomType);
    }

    /// <summary>
    /// Xóa loại phòng (soft delete - set IsActive = false)
    /// DELETE /api/room-types/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRoomType(int id)
    {
        var roomType = await _context.RoomTypes
            .Include(rt => rt.Rooms)
            .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

        if (roomType == null)
        {
            return NotFound(new { message = "Không tìm thấy loại phòng" });
        }

        // Soft delete - chỉ set IsActive = false
        roomType.IsActive = false;
        roomType.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Audit log
        var username = User.Identity?.Name ?? "System";
        await _auditService.LogAsync(
            "RoomType",
            roomType.RoomTypeId,
            "SoftDelete",
            username,
            Newtonsoft.Json.JsonConvert.SerializeObject(roomType),
            null,
            $"Xóa mềm loại phòng: {roomType.TypeName}"
        );

        _logger.LogInformation($"[RoomTypes] Soft deleted room type: {roomType.TypeName} by {username}");

        return Ok(new { 
            message = "Đã xóa mềm loại phòng thành công",
            isActive = roomType.IsActive,
            roomCount = roomType.Rooms.Count
        });
    }

    /// <summary>
    /// Kích hoạt/vô hiệu hóa loại phòng
    /// PATCH /api/room-types/{id}/toggle-active
    /// </summary>
    [HttpPatch("{id}/toggle-active")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var roomType = await _context.RoomTypes.FindAsync(id);
        if (roomType == null)
        {
            return NotFound(new { message = "Không tìm thấy loại phòng" });
        }

        roomType.IsActive = !roomType.IsActive;
        roomType.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var username = User.Identity?.Name ?? "System";
        await _auditService.LogAsync(
            "RoomType",
            roomType.RoomTypeId,
            "Update",
            username,
            null,
            null,
            $"Thay đổi trạng thái loại phòng '{roomType.TypeName}' thành {(roomType.IsActive ? "Active" : "Inactive")}"
        );

        return Ok(new
        {
            message = $"Đã {(roomType.IsActive ? "kích hoạt" : "vô hiệu hóa")} loại phòng",
            isActive = roomType.IsActive
        });
    }

    /// <summary>
    /// Lấy thống kê loại phòng
    /// GET /api/room-types/statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Manager,Business")]
    public async Task<IActionResult> GetStatistics()
    {
        var roomTypes = await _context.RoomTypes
            .Include(rt => rt.Rooms)
            .ToListAsync();

        var stats = roomTypes.Select(rt => new
        {
            roomTypeId = rt.RoomTypeId,
            typeName = rt.TypeName,
            typeCode = rt.TypeCode,
            basePrice = rt.BasePrice,
            totalRooms = rt.Rooms.Count,
            availableRooms = rt.Rooms.Count(r => r.IsAvailable),
            occupiedRooms = rt.Rooms.Count(r => !r.IsAvailable),
            isActive = rt.IsActive
        }).ToList();

        return Ok(new
        {
            totalRoomTypes = roomTypes.Count,
            activeRoomTypes = roomTypes.Count(rt => rt.IsActive),
            inactiveRoomTypes = roomTypes.Count(rt => !rt.IsActive),
            roomTypes = stats
        });
    }
}

