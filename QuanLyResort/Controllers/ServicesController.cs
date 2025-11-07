using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Services;
using System.IO;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/services")]
[Authorize(Roles = "Admin,Manager")]
public class ServicesController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly IAuditService _auditService;

    public ServicesController(ResortDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    /// <summary>
    /// Lấy danh sách menu nhà hàng (public endpoint)
    /// GET /api/services/restaurant/menu
    /// </summary>
    [HttpGet("restaurant/menu")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRestaurantMenu()
    {
        try
        {
            // Get all restaurant services
            var allRestaurantServices = await _context.Services
                .Where(s => s.ServiceType == "Restaurant")
                .ToListAsync();

            // Filter active ones
            var menuItems = allRestaurantServices
                .Where(s => s.IsActive)
                .OrderBy(s => s.ServiceName)
                .Select(s => new
                {
                    s.ServiceId,
                    s.ServiceName,
                    s.Description,
                    s.Price,
                    s.Unit,
                    s.ImageUrl
                })
                .ToList();

            Console.WriteLine($"[GetRestaurantMenu] Total Restaurant services in DB: {allRestaurantServices.Count}, Active: {menuItems.Count}");

            return Ok(menuItems);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetRestaurantMenu] Error: {ex.Message}");
            return StatusCode(500, new { message = "Failed to load restaurant menu", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách dịch vụ
    /// GET /api/services
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllServices([FromQuery] string? search = null, [FromQuery] string? type = null, [FromQuery] bool? isActive = null)
    {
        var query = _context.Services.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s => s.ServiceName.Contains(search) || 
                                      (s.Description != null && s.Description.Contains(search)));
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(s => s.ServiceType == type);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        var services = await query.OrderBy(s => s.ServiceName).ToListAsync();
        return Ok(services);
    }

    /// <summary>
    /// Lấy chi tiết dịch vụ
    /// GET /api/services/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetServiceById(int id)
    {
        var service = await _context.Services.FindAsync(id);
        
        if (service == null)
        {
            return NotFound(new { message = "Service not found." });
        }

        return Ok(service);
    }

    /// <summary>
    /// Lấy thống kê dịch vụ
    /// GET /api/services/statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetServiceStatistics()
    {
        var totalServices = await _context.Services.CountAsync();
        var activeServices = await _context.Services.CountAsync(s => s.IsActive);
        var inactiveServices = totalServices - activeServices;
        
        // SQLite doesn't support decimal Sum directly, use client-side aggregation
        var totalRevenue = (await _context.Charges
            .Where(c => c.ChargeType == "ServiceCharge")
            .ToListAsync())
            .Sum(c => (decimal?)c.TotalAmount) ?? 0;

        return Ok(new
        {
            totalServices,
            activeServices,
            inactiveServices,
            totalRevenue
        });
    }

    /// <summary>
    /// Tạo dịch vụ mới
    /// POST /api/services
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateService([FromBody] Service service)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        service.CreatedAt = DateTime.UtcNow;
        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Service", service.ServiceId, "Create", GetCurrentUsername(), null, System.Text.Json.JsonSerializer.Serialize(service));

        return CreatedAtAction(nameof(GetServiceById), new { id = service.ServiceId }, service);
    }

    /// <summary>
    /// Cập nhật dịch vụ
    /// PUT /api/services/{id}
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] Service service)
    {
        if (id != service.ServiceId)
        {
            return BadRequest(new { message = "Service ID mismatch." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingService = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.ServiceId == id);
        if (existingService == null)
        {
            return NotFound(new { message = "Service not found." });
        }

        var oldData = System.Text.Json.JsonSerializer.Serialize(existingService);

        service.UpdatedAt = DateTime.UtcNow;
        _context.Entry(service).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Service", service.ServiceId, "Update", GetCurrentUsername(), oldData, System.Text.Json.JsonSerializer.Serialize(service));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Services.AnyAsync(s => s.ServiceId == id))
            {
                return NotFound(new { message = "Service not found." });
            }
            throw;
        }

        return Ok(new { message = "Service updated successfully.", service });
    }

    /// <summary>
    /// Xóa dịch vụ
    /// DELETE /api/services/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        
        if (service == null)
        {
            return NotFound(new { message = "Service not found." });
        }

        // Check if service is being used in charges
        var hasCharges = await _context.Charges.AnyAsync(c => c.ServiceId == id);
        if (hasCharges)
        {
            return BadRequest(new { message = "Cannot delete service that has been used in charges." });
        }

        var oldData = System.Text.Json.JsonSerializer.Serialize(service);

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Service", id, "Delete", GetCurrentUsername(), oldData, null);

        return Ok(new { message = "Service deleted successfully." });
    }

    /// <summary>
    /// Toggle trạng thái hoạt động của dịch vụ
    /// PATCH /api/services/{id}/toggle-active
    /// </summary>
    [HttpPatch("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var service = await _context.Services.FindAsync(id);
        
        if (service == null)
        {
            return NotFound(new { message = "Service not found." });
        }

        var oldData = System.Text.Json.JsonSerializer.Serialize(service);
        service.IsActive = !service.IsActive;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Service", id, "ToggleActive", GetCurrentUsername(), oldData, System.Text.Json.JsonSerializer.Serialize(service));

        return Ok(new { message = $"Service {(service.IsActive ? "activated" : "deactivated")} successfully.", service });
    }

    /// <summary>
    /// Lấy danh sách các loại dịch vụ
    /// GET /api/services/types
    /// </summary>
    [HttpGet("types")]
    [AllowAnonymous]
    public async Task<IActionResult> GetServiceTypes()
    {
        var types = await _context.Services
            .Select(s => s.ServiceType)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        return Ok(types);
    }

    /// <summary>
    /// Upload hình ảnh cho dịch vụ
    /// POST /api/services/{id}/upload-image
    /// </summary>
    [HttpPost("{id}/upload-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadServiceImage(int id, [FromForm] IFormFile? file)
    {
        try
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound(new { message = "Service not found." });
            }

            // Lưu oldImageUrl để dùng cho cả hai trường hợp (xóa và upload mới)
            var oldImageUrl = service.ImageUrl;

            // Nếu không có file, xóa image URL
            if (file == null || file.Length == 0)
            {
                service.ImageUrl = null;
                service.UpdatedAt = DateTime.UtcNow;
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

                await _auditService.LogAsync("Service", id, "RemoveImage", GetCurrentUsername(), oldImageUrl, null);
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
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "services");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileName = $"service_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
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

            // Update service with new image URL
            var imageUrl = $"/uploads/services/{fileName}";
            service.ImageUrl = imageUrl;
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Service", id, "UploadImage", GetCurrentUsername(), oldImageUrl, imageUrl);

            return Ok(new { message = "Image uploaded successfully.", imageUrl });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UploadServiceImage] Error: {ex.Message}");
            return StatusCode(500, new { message = "Failed to upload image.", error = ex.Message });
        }
    }

    private string GetCurrentUsername()
    {
        return User.Identity?.Name ?? "Unknown";
    }
}

