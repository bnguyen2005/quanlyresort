using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Services;
using System.Security.Claims;
using System.IO;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(
        ResortDbContext context,
        IAuditService auditService,
        ILogger<RoomsController> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả phòng với filter
    /// GET /api/rooms?status=Available&roomTypeId=1
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllRooms(
        [FromQuery] string? status = null, // Available, Occupied, Cleaning, Maintenance
        [FromQuery] int? roomTypeId = null,
        [FromQuery] string? floor = null,
        [FromQuery] bool? isAvailable = null)
    {
        var query = _context.Rooms
            .Include(r => r.RoomTypeNavigation)
            .AsQueryable();

        // Filter by availability
        if (isAvailable.HasValue)
        {
            query = query.Where(r => r.IsAvailable == isAvailable.Value);
        }

        // Filter by housekeeping status
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.HousekeepingStatus == status);
        }

        // Filter by room type
        if (roomTypeId.HasValue)
        {
            query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
        }

        // Filter by floor
        if (!string.IsNullOrEmpty(floor))
        {
            query = query.Where(r => r.Floor == floor);
        }

        var rooms = await query
            .OrderBy(r => r.Floor)
            .ThenBy(r => r.RoomNumber)
            .Select(r => new
            {
                r.RoomId,
                r.RoomNumber,
                r.RoomType,
                r.RoomTypeId,
                RoomTypeName = r.RoomTypeNavigation != null ? r.RoomTypeNavigation.TypeName : null,
                RoomTypeImageUrl = r.RoomTypeNavigation != null ? r.RoomTypeNavigation.MainImageUrl : null,
                RoomTypeImageGallery = r.RoomTypeNavigation != null ? r.RoomTypeNavigation.ImageGallery : null,
                RoomTypeRoomSize = r.RoomTypeNavigation != null ? r.RoomTypeNavigation.RoomSize : null,
                r.Floor,
                r.PricePerNight,
                r.MaxOccupancy,
                r.Description,
                r.Amenities,
                r.IsAvailable,
                r.HousekeepingStatus,
                r.Notes,
                r.ImageUrl,
                r.ImageGallery,
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(rooms);
    }

    /// <summary>
    /// Lấy chi tiết phòng
    /// GET /api/rooms/{id}
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoomById(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.RoomTypeNavigation)
            .Where(r => r.RoomId == id)
            .Select(r => new
            {
                r.RoomId,
                r.RoomNumber,
                r.RoomType,
                r.RoomTypeId,
                RoomTypeName = r.RoomTypeNavigation != null ? r.RoomTypeNavigation.TypeName : null,
                RoomTypeImageUrl = r.RoomTypeNavigation != null ? r.RoomTypeNavigation.MainImageUrl : null,
                RoomTypeImageGallery = r.RoomTypeNavigation != null ? r.RoomTypeNavigation.ImageGallery : null,
                RoomTypeRoomSize = r.RoomTypeNavigation != null ? r.RoomTypeNavigation.RoomSize : null,
                RoomTypeDetails = r.RoomTypeNavigation,
                r.Floor,
                r.PricePerNight,
                r.MaxOccupancy,
                r.Description,
                r.Amenities,
                r.IsAvailable,
                r.HousekeepingStatus,
                r.Notes,
                r.ImageUrl,
                r.ImageGallery,
                r.CreatedAt,
                r.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (room == null)
        {
            return NotFound(new { message = "Room not found." });
        }

        return Ok(room);
    }

    /// <summary>
    /// Lấy thống kê phòng
    /// GET /api/rooms/statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Manager,Business,FrontDesk")]
    public async Task<IActionResult> GetRoomStatistics()
    {
        var totalRooms = await _context.Rooms.CountAsync();
        var availableRooms = await _context.Rooms.CountAsync(r => r.IsAvailable);
        var occupiedRooms = await _context.Rooms.CountAsync(r => !r.IsAvailable && r.HousekeepingStatus == "Clean");
        var cleaningRooms = await _context.Rooms.CountAsync(r => r.HousekeepingStatus == "InProgress" || r.HousekeepingStatus == "Dirty");
        var maintenanceRooms = await _context.Rooms.CountAsync(r => r.HousekeepingStatus == "Maintenance");

        var byRoomType = await _context.Rooms
            .GroupBy(r => new { r.RoomType, r.RoomTypeId })
            .Select(g => new
            {
                roomType = g.Key.RoomType,
                roomTypeId = g.Key.RoomTypeId,
                total = g.Count(),
                available = g.Count(r => r.IsAvailable),
                occupied = g.Count(r => !r.IsAvailable)
            })
            .OrderBy(x => x.roomType)
            .ToListAsync();

        var byFloor = await _context.Rooms
            .GroupBy(r => r.Floor)
            .Select(g => new
            {
                floor = g.Key,
                total = g.Count(),
                available = g.Count(r => r.IsAvailable),
                occupied = g.Count(r => !r.IsAvailable)
            })
            .OrderBy(x => x.floor)
            .ToListAsync();

        return Ok(new
        {
            totalRooms,
            availableRooms,
            occupiedRooms,
            cleaningRooms,
            maintenanceRooms,
            byRoomType,
            byFloor
        });
    }

    /// <summary>
    /// Lấy danh sách tầng
    /// GET /api/rooms/floors
    /// </summary>
    [HttpGet("floors")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFloors()
    {
        var floors = await _context.Rooms
            .Select(r => r.Floor)
            .Distinct()
            .OrderBy(f => f)
            .ToListAsync();

        return Ok(floors);
    }

    /// <summary>
    /// Upload hình ảnh cho phòng
    /// POST /api/rooms/{id}/upload-image
    /// </summary>
    [HttpPost("{id:int}/upload-image")]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadRoomImage(int id, [FromForm] IFormFile? file)
    {
        try
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound(new { message = "Room not found." });
            }

            // Lưu oldImageUrl để dùng cho cả hai trường hợp (xóa và upload mới)
            var oldImageUrl = room.ImageUrl;

            // Nếu không có file, xóa image URL
            if (file == null || file.Length == 0)
            {
                room.ImageUrl = null;
                room.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(oldImageUrl) && oldImageUrl.StartsWith("/"))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try { System.IO.File.Delete(oldFilePath); } catch { }
                    }
                }

                await _auditService.LogAsync("Room", id, "RemoveImage", GetCurrentUsername() ?? "Unknown", oldImageUrl, null);
                return Ok(new { message = "Image removed successfully.", imageUrl = (string?)null });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed: JPG, JPEG, PNG, GIF, WEBP" });
            }

            // Validate file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = "File size exceeds 5MB limit." });
            }

            // Tạo thư mục uploads nếu chưa có
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "rooms");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileName = $"room_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Xóa file cũ nếu có (oldImageUrl đã được khai báo ở đầu hàm)
            if (!string.IsNullOrEmpty(oldImageUrl) && oldImageUrl.StartsWith("/"))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath) && oldFilePath != filePath)
                {
                    try { System.IO.File.Delete(oldFilePath); } catch { }
                }
            }

            // Update room with new image URL
            var imageUrl = $"/uploads/rooms/{fileName}";
            room.ImageUrl = imageUrl;
            room.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Room", id, "UploadImage", GetCurrentUsername() ?? "Unknown", oldImageUrl, imageUrl);

            return Ok(new { message = "Image uploaded successfully.", imageUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading room image");
            return StatusCode(500, new { message = "Failed to upload image.", error = ex.Message });
        }
    }

    /// <summary>
    /// Thêm hình ảnh vào gallery của phòng
    /// POST /api/rooms/{id}/gallery/add
    /// </summary>
    [HttpPost("{id:int}/gallery/add")]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddImageToGallery(int id, [FromForm] IFormFile? file)
    {
        try
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound(new { message = "Room not found." });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided or file is empty." });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed: JPG, JPEG, PNG, GIF, WEBP" });
            }

            // Validate file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = "File size exceeds 5MB limit." });
            }

            // Parse existing gallery or create new
            List<string> gallery = new List<string>();
            if (!string.IsNullOrEmpty(room.ImageGallery))
            {
                try
                {
                    gallery = System.Text.Json.JsonSerializer.Deserialize<List<string>>(room.ImageGallery) ?? new List<string>();
                }
                catch
                {
                    gallery = new List<string>();
                }
            }

            // Check max 5 images
            if (gallery.Count >= 5)
            {
                return BadRequest(new { message = "Gallery đã đầy. Tối đa 5 hình ảnh. Vui lòng xóa một hình ảnh trước khi thêm mới." });
            }

            // Tạo thư mục uploads nếu chưa có
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "rooms", "gallery");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileName = $"room_{id}_gallery_{DateTime.UtcNow:yyyyMMddHHmmss}_{gallery.Count + 1}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Add to gallery
            var imageUrl = $"/uploads/rooms/gallery/{fileName}";
            gallery.Add(imageUrl);

            // Update room
            room.ImageGallery = System.Text.Json.JsonSerializer.Serialize(gallery);
            room.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Room", id, "AddImageToGallery", GetCurrentUsername() ?? "Unknown", room.ImageGallery, imageUrl);

            return Ok(new { message = "Image added to gallery successfully.", imageUrl, gallery });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding image to gallery");
            return StatusCode(500, new { message = "Failed to add image to gallery.", error = ex.Message });
        }
    }

    /// <summary>
    /// Xóa hình ảnh khỏi gallery của phòng
    /// DELETE /api/rooms/{id}/gallery/remove?imageUrl=...
    /// </summary>
    [HttpDelete("{id:int}/gallery/remove")]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    public async Task<IActionResult> RemoveImageFromGallery(int id, [FromQuery] string imageUrl)
    {
        try
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound(new { message = "Room not found." });
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest(new { message = "imageUrl parameter is required." });
            }

            // Parse gallery
            List<string> gallery = new List<string>();
            if (!string.IsNullOrEmpty(room.ImageGallery))
            {
                try
                {
                    gallery = System.Text.Json.JsonSerializer.Deserialize<List<string>>(room.ImageGallery) ?? new List<string>();
                }
                catch
                {
                    return BadRequest(new { message = "Invalid gallery format." });
                }
            }

            // Remove image from gallery
            var oldGallery = System.Text.Json.JsonSerializer.Serialize(gallery);
            gallery = gallery.Where(url => url != imageUrl && !url.EndsWith(imageUrl)).ToList();

            // Delete file from disk
            if (imageUrl.StartsWith("/"))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    try { System.IO.File.Delete(filePath); } catch { }
                }
            }

            // Update room
            room.ImageGallery = gallery.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(gallery) : null;
            room.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Room", id, "RemoveImageFromGallery", GetCurrentUsername() ?? "Unknown", oldGallery, room.ImageGallery);

            return Ok(new { message = "Image removed from gallery successfully.", gallery });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing image from gallery");
            return StatusCode(500, new { message = "Failed to remove image from gallery.", error = ex.Message });
        }
    }

    /// <summary>
    /// Tạo phòng mới
    /// POST /api/rooms
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateRoom([FromBody] Room room)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check for duplicate room number
        if (await _context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber))
        {
            return Conflict(new { message = "Room number already exists." });
        }

        // Validate RoomTypeId if provided
        if (room.RoomTypeId.HasValue)
        {
            var roomTypeExists = await _context.RoomTypes.AnyAsync(rt => rt.RoomTypeId == room.RoomTypeId.Value);
            if (!roomTypeExists)
            {
                return BadRequest(new { message = "Invalid RoomTypeId." });
            }
        }

        room.CreatedAt = DateTime.UtcNow;
        room.IsAvailable = true;
        room.HousekeepingStatus = "Ready";

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Room", room.RoomId, "Create", GetCurrentUsername(), null, System.Text.Json.JsonSerializer.Serialize(room));

        // Return room with explicit roomId for frontend
        return CreatedAtAction(nameof(GetRoomById), new { id = room.RoomId }, new { 
            roomId = room.RoomId,
            room = room 
        });
    }

    /// <summary>
    /// Cập nhật thông tin phòng
    /// PUT /api/rooms/{id}
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] Room room)
    {
        if (id != room.RoomId)
        {
            return BadRequest(new { message = "Room ID mismatch." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingRoom = await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.RoomId == id);
        if (existingRoom == null)
        {
            return NotFound(new { message = "Room not found." });
        }

        // Check for duplicate room number if changed
        if (room.RoomNumber != existingRoom.RoomNumber)
        {
            if (await _context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber && r.RoomId != id))
            {
                return Conflict(new { message = "Room number already exists." });
            }
        }

        // Validate RoomTypeId if changed
        if (room.RoomTypeId.HasValue && room.RoomTypeId != existingRoom.RoomTypeId)
        {
            var roomTypeExists = await _context.RoomTypes.AnyAsync(rt => rt.RoomTypeId == room.RoomTypeId.Value);
            if (!roomTypeExists)
            {
                return BadRequest(new { message = "Invalid RoomTypeId." });
            }
        }

        room.UpdatedAt = DateTime.UtcNow;
        _context.Entry(room).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Room", room.RoomId, "Update", GetCurrentUsername(), System.Text.Json.JsonSerializer.Serialize(existingRoom), System.Text.Json.JsonSerializer.Serialize(room));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RoomExists(id))
            {
                return NotFound(new { message = "Room not found." });
            }
            else
            {
                throw;
            }
        }

        return Ok(new { message = "Room updated successfully.", room });
    }

    /// <summary>
    /// Cập nhật trạng thái phòng
    /// PATCH /api/rooms/{id}/status
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Manager,FrontDesk")]
    public async Task<IActionResult> UpdateRoomStatus(int id, [FromBody] RoomStatusUpdateRequest request)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound(new { message = "Room not found." });
        }

        var oldStatus = new { room.IsAvailable, room.HousekeepingStatus };

        room.IsAvailable = request.IsAvailable;
        room.HousekeepingStatus = request.HousekeepingStatus;
        room.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.Notes))
        {
            room.Notes = request.Notes;
        }

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Room", room.RoomId, "UpdateStatus", GetCurrentUsername(), System.Text.Json.JsonSerializer.Serialize(oldStatus), System.Text.Json.JsonSerializer.Serialize(new { room.IsAvailable, room.HousekeepingStatus }), $"Room {room.RoomNumber} status updated");

        return Ok(new { message = "Room status updated successfully.", room });
    }

    /// <summary>
    /// Xóa phòng
    /// DELETE /api/rooms/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound(new { message = "Room not found." });
        }

        // Check if room has active bookings
        var hasActiveBookings = await _context.Bookings.AnyAsync(b => b.RoomId == id && b.Status != "Cancelled" && b.Status != "CheckedOut");
        if (hasActiveBookings)
        {
            return Conflict(new { message = $"Cannot delete room '{room.RoomNumber}' because it has active bookings. Please cancel or complete all bookings first." });
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Room", room.RoomId, "Delete", GetCurrentUsername(), System.Text.Json.JsonSerializer.Serialize(room), null, $"Deleted Room: {room.RoomNumber}");

        return Ok(new { message = "Room deleted successfully." });
    }

    private bool RoomExists(int id)
    {
        return _context.Rooms.Any(e => e.RoomId == id);
    }

    private string? GetCurrentUsername()
    {
        return HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? HttpContext.User.FindFirst("Username")?.Value;
    }
}

public class RoomStatusUpdateRequest
{
    public bool IsAvailable { get; set; }
    public string HousekeepingStatus { get; set; } = "Ready"; // Ready, Clean, Dirty, InProgress, Maintenance
    public string? Notes { get; set; }
}
