using QuanLyResort.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Services;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/room-types")]
[Authorize] // Yêu c?u authentication
public class RoomTypesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private ResortDbContext _context => _unitOfWork.Context;
    private readonly IAuditService _auditService;
    private readonly ILogger<RoomTypesController> _logger;

    public RoomTypesController(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ILogger<RoomTypesController> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// L?y danh sách t?t c? lo?i phòng
    /// GET /api/room-types
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // Khách vãng lai cung có th? xem
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
    /// L?y thông tin chi ti?t m?t lo?i phòng
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
                .Include(rt => rt.Rooms) // Include rooms d? xem có bao nhiêu phòng thu?c lo?i này
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == id);

            if (roomType == null)
            {
                _logger.LogWarning($"[RoomTypes] Room type {id} not found");
                return NotFound(new { message = "Không tìm th?y lo?i phòng" });
            }

            _logger.LogInformation($"[RoomTypes] Found room type: {roomType.TypeName}, Rooms count: {roomType.Rooms?.Count ?? 0}");

            // Thêm th?ng kê s? phòng
            var stats = new
            {
                totalRooms = roomType.Rooms?.Count ?? 0,
                availableRooms = roomType.Rooms?.Count(r => r.IsAvailable) ?? 0
            };

            _logger.LogInformation($"[RoomTypes] Returning stats: Total={stats.totalRooms}, Available={stats.availableRooms}");

            // T?o response object d? tránh circular reference
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
            return StatusCode(500, new { message = "L?i server khi l?y thông tin lo?i phòng", error = ex.Message });
        }
    }

    /// <summary>
    /// T?o lo?i phòng m?i
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

        // Ki?m tra TypeCode dã t?n t?i chua
        var existingCode = await _context.RoomTypes
            .AnyAsync(rt => rt.TypeCode.ToLower() == roomType.TypeCode.ToLower());

        if (existingCode)
        {
            return BadRequest(new { message = $"Mã lo?i phòng '{roomType.TypeCode}' dã t?n t?i" });
        }

        roomType.CreatedAt = DateTime.UtcNow;
        roomType.UpdatedAt = null;

        _context.RoomTypes.Add(roomType);
        await _unitOfWork.SaveChangesAsync();

        // Audit log
        var username = User.Identity?.Name ?? "System";
        await _auditService.LogAsync(
            "RoomType",
            roomType.RoomTypeId,
            "Create",
            username,
            null,
            Newtonsoft.Json.JsonConvert.SerializeObject(roomType),
            $"T?o lo?i phòng m?i: {roomType.TypeName}"
        );

        _logger.LogInformation($"[RoomTypes] Created new room type: {roomType.TypeName} by {username}");

        return CreatedAtAction(
            nameof(GetRoomTypeById),
            new { id = roomType.RoomTypeId },
            roomType
        );
    }

    /// <summary>
    /// C?p nh?t lo?i phòng
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
            return NotFound(new { message = "Không tìm th?y lo?i phòng" });
        }

        // Ki?m tra TypeCode trùng (ngo?i tr? chính nó)
        var duplicateCode = await _context.RoomTypes
            .AnyAsync(rt => rt.TypeCode.ToLower() == updatedRoomType.TypeCode.ToLower() 
                         && rt.RoomTypeId != id);

        if (duplicateCode)
        {
            return BadRequest(new { message = $"Mã lo?i phòng '{updatedRoomType.TypeCode}' dã t?n t?i" });
        }

        // Luu old values cho audit
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

        await _unitOfWork.SaveChangesAsync();

        // Audit log
        var username = User.Identity?.Name ?? "System";
        await _auditService.LogAsync(
            "RoomType",
            roomType.RoomTypeId,
            "Update",
            username,
            oldValues,
            Newtonsoft.Json.JsonConvert.SerializeObject(roomType),
            $"C?p nh?t lo?i phòng: {roomType.TypeName}"
        );

        _logger.LogInformation($"[RoomTypes] Updated room type: {roomType.TypeName} by {username}");

        return Ok(roomType);
    }

    /// <summary>
    /// Xóa lo?i phòng (soft delete - set IsActive = false)
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
            return NotFound(new { message = "Không tìm th?y lo?i phòng" });
        }

        // Soft delete - ch? set IsActive = false
        roomType.IsActive = false;
        roomType.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // Audit log
        var username = User.Identity?.Name ?? "System";
        await _auditService.LogAsync(
            "RoomType",
            roomType.RoomTypeId,
            "SoftDelete",
            username,
            Newtonsoft.Json.JsonConvert.SerializeObject(roomType),
            null,
            $"Xóa m?m lo?i phòng: {roomType.TypeName}"
        );

        _logger.LogInformation($"[RoomTypes] Soft deleted room type: {roomType.TypeName} by {username}");

        return Ok(new { 
            message = "Ðã xóa m?m lo?i phòng thành công",
            isActive = roomType.IsActive,
            roomCount = roomType.Rooms.Count
        });
    }

    /// <summary>
    /// Kích ho?t/vô hi?u hóa lo?i phòng
    /// PATCH /api/room-types/{id}/toggle-active
    /// </summary>
    [HttpPatch("{id}/toggle-active")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var roomType = await _context.RoomTypes.FindAsync(id);
        if (roomType == null)
        {
            return NotFound(new { message = "Không tìm th?y lo?i phòng" });
        }

        roomType.IsActive = !roomType.IsActive;
        roomType.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        var username = User.Identity?.Name ?? "System";
        await _auditService.LogAsync(
            "RoomType",
            roomType.RoomTypeId,
            "Update",
            username,
            null,
            null,
            $"Thay d?i tr?ng thái lo?i phòng '{roomType.TypeName}' thành {(roomType.IsActive ? "Active" : "Inactive")}"
        );

        return Ok(new
        {
            message = $"Ðã {(roomType.IsActive ? "kích ho?t" : "vô hi?u hóa")} lo?i phòng",
            isActive = roomType.IsActive
        });
    }

    /// <summary>
    /// L?y th?ng kê lo?i phòng
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


