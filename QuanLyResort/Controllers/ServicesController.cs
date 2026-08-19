using QuanLyResort.Repositories;
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
    private readonly IUnitOfWork _unitOfWork;
    private ResortDbContext _context => _unitOfWork.Context;
    private readonly IAuditService _auditService;

    public ServicesController(IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    /// <summary>
    /// L?y danh sách menu nhà hàng (public endpoint)
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
    /// L?y danh sách d?ch v?
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
    /// L?y chi ti?t d?ch v?
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
    /// L?y th?ng kê d?ch v?
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
    /// T?o d?ch v? m?i
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
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Service", service.ServiceId, "Create", GetCurrentUsername(), null, System.Text.Json.JsonSerializer.Serialize(service));

        return CreatedAtAction(nameof(GetServiceById), new { id = service.ServiceId }, service);
    }

    /// <summary>
    /// C?p nh?t d?ch v?
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
            await _unitOfWork.SaveChangesAsync();
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
    /// Xóa d?ch v?
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
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Service", id, "Delete", GetCurrentUsername(), oldData, null);

        return Ok(new { message = "Service deleted successfully." });
    }

    /// <summary>
    /// Toggle tr?ng thái ho?t d?ng c?a d?ch v?
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

        await _unitOfWork.SaveChangesAsync();
        await _auditService.LogAsync("Service", id, "ToggleActive", GetCurrentUsername(), oldData, System.Text.Json.JsonSerializer.Serialize(service));

        return Ok(new { message = $"Service {(service.IsActive ? "activated" : "deactivated")} successfully.", service });
    }

    /// <summary>
    /// L?y danh sách các lo?i d?ch v?
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
    /// Upload hình ?nh cho d?ch v?
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

            // Luu oldImageUrl d? dùng cho c? hai tru?ng h?p (xóa và upload m?i)
            var oldImageUrl = service.ImageUrl;

            // N?u không có file, xóa image URL
            if (file == null || file.Length == 0)
            {
                service.ImageUrl = null;
                service.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                // Xóa file cu n?u có
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

            // T?o thu m?c uploads n?u chua có
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

            // Xóa file cu n?u có (oldImageUrl dã du?c khai báo ? d?u hàm)
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
            await _unitOfWork.SaveChangesAsync();

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


