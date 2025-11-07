using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using System.Security.Claims;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ResortDbContext _context;

    public CouponsController(ResortDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Validate a coupon code (for customers)
    /// </summary>
    [HttpGet("validate")]
    [AllowAnonymous] // Allow customers to validate without auth
    public async Task<IActionResult> ValidateCoupon([FromQuery] string code)
    {
        // 1. Validate input format
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { message = "Mã giảm giá không được để trống" });
        }

        // Normalize code: uppercase, trim, remove spaces
        var normalizedCode = code.ToUpper().Trim().Replace(" ", "");
        
        // Validate format: alphanumeric, 3-50 characters
        if (normalizedCode.Length < 3 || normalizedCode.Length > 50)
        {
            return BadRequest(new { message = "Mã giảm giá phải có từ 3 đến 50 ký tự" });
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedCode, @"^[A-Z0-9]+$"))
        {
            return BadRequest(new { message = "Mã giảm giá chỉ được chứa chữ cái và số" });
        }

        // 2. Find coupon in database
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == normalizedCode);

        if (coupon == null)
        {
            return NotFound(new { message = "Mã giảm giá không tồn tại" });
        }

        // 3. Validate coupon status and dates
        var now = DateTime.UtcNow;
        var dateFormat = "dd/MM/yyyy";

        if (!coupon.IsActive)
        {
            return BadRequest(new { 
                message = "Mã giảm giá đã bị tắt",
                code = coupon.Code,
                isActive = false
            });
        }

        if (now < coupon.StartDate)
        {
            return BadRequest(new { 
                message = $"Mã giảm giá chưa có hiệu lực. Mã sẽ có hiệu lực từ {coupon.StartDate.ToString(dateFormat)}",
                code = coupon.Code,
                startDate = coupon.StartDate,
                endDate = coupon.EndDate
            });
        }

        if (now > coupon.EndDate)
        {
            return BadRequest(new { 
                message = $"Mã giảm giá đã hết hạn (hết hạn: {coupon.EndDate.ToString(dateFormat)})",
                code = coupon.Code,
                endDate = coupon.EndDate
            });
        }

        // 4. Validate usage limits
        if (coupon.MaxUses.HasValue && coupon.UsesCount >= coupon.MaxUses.Value)
        {
            return BadRequest(new { 
                message = $"Mã giảm giá đã hết lượt sử dụng ({coupon.UsesCount}/{coupon.MaxUses})",
                code = coupon.Code,
                usesCount = coupon.UsesCount,
                maxUses = coupon.MaxUses
            });
        }

        // 5. Validate coupon value
        if (coupon.Value <= 0)
        {
            return BadRequest(new { message = "Mã giảm giá có giá trị không hợp lệ" });
        }

        if (coupon.Type.ToLower() == "percent" && (coupon.Value < 1 || coupon.Value > 100))
        {
            return BadRequest(new { message = "Mã giảm giá phần trăm phải từ 1% đến 100%" });
        }

        // 6. Return validated coupon info (without sensitive data)
        return Ok(new
        {
            code = coupon.Code,
            type = coupon.Type,
            value = coupon.Value,
            maxDiscount = coupon.MaxDiscount,
            description = coupon.Description,
            startDate = coupon.StartDate,
            endDate = coupon.EndDate,
            usesCount = coupon.UsesCount,
            maxUses = coupon.MaxUses
        });
    }

    /// <summary>
    /// Get active coupons for customers (public endpoint)
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveCoupons()
    {
        var now = DateTime.UtcNow;
        
        var activeCoupons = await _context.Coupons
            .Where(c => c.IsActive 
                && c.StartDate <= now 
                && c.EndDate >= now
                && (!c.MaxUses.HasValue || c.UsesCount < c.MaxUses.Value))
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                code = c.Code,
                description = c.Description,
                type = c.Type,
                value = c.Value,
                maxDiscount = c.MaxDiscount,
                startDate = c.StartDate,
                endDate = c.EndDate
            })
            .ToListAsync();

        return Ok(activeCoupons);
    }

    /// <summary>
    /// Get all coupons (Admin/Manager only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetCoupons(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? type = null)
    {
        var query = _context.Coupons.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(c => c.Type.ToLower() == type.ToLower());
        }

        var coupons = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(coupons);
    }

    /// <summary>
    /// Get coupon by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetCouponById(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);

        if (coupon == null)
        {
            return NotFound(new { message = "Không tìm thấy mã giảm giá" });
        }

        return Ok(coupon);
    }

    /// <summary>
    /// Create a new coupon
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";

        // Validate
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new { message = "Mã giảm giá không được để trống" });
        }

        // Check if code already exists
        var existingCoupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == request.Code.ToUpper().Trim());

        if (existingCoupon != null)
        {
            return BadRequest(new { message = "Mã giảm giá đã tồn tại" });
        }

        // Validate type and value
        if (request.Type.ToLower() == "percent")
        {
            if (request.Value < 1 || request.Value > 100)
            {
                return BadRequest(new { message = "Giá trị phần trăm phải từ 1 đến 100" });
            }
        }
        else if (request.Type.ToLower() == "amount")
        {
            if (request.Value <= 0)
            {
                return BadRequest(new { message = "Số tiền giảm phải lớn hơn 0" });
            }
        }
        else
        {
            return BadRequest(new { message = "Loại giảm giá không hợp lệ (phải là 'percent' hoặc 'amount')" });
        }

        if (request.EndDate <= request.StartDate)
        {
            return BadRequest(new { message = "Ngày kết thúc phải sau ngày bắt đầu" });
        }

        var coupon = new Coupon
        {
            Code = request.Code.ToUpper().Trim(),
            Description = request.Description?.Trim(),
            Type = request.Type.ToLower(),
            Value = request.Value,
            MaxDiscount = request.MaxDiscount,
            MaxUses = request.MaxUses ?? 0, // 0 = unlimited
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userEmail
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCouponById), new { id = coupon.CouponId }, coupon);
    }

    /// <summary>
    /// Update a coupon
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateCoupon(int id, [FromBody] UpdateCouponRequest request)
    {
        var coupon = await _context.Coupons.FindAsync(id);

        if (coupon == null)
        {
            return NotFound(new { message = "Không tìm thấy mã giảm giá" });
        }

        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";

        // Validate code uniqueness if changed
        if (!string.IsNullOrWhiteSpace(request.Code) && request.Code.ToUpper().Trim() != coupon.Code)
        {
            var existingCoupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == request.Code.ToUpper().Trim() && c.CouponId != id);

            if (existingCoupon != null)
            {
                return BadRequest(new { message = "Mã giảm giá đã tồn tại" });
            }

            coupon.Code = request.Code.ToUpper().Trim();
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            coupon.Description = request.Description.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            coupon.Type = request.Type.ToLower();
        }

        if (request.Value.HasValue)
        {
            coupon.Value = request.Value.Value;
        }

        if (request.MaxDiscount.HasValue)
        {
            coupon.MaxDiscount = request.MaxDiscount.Value;
        }

        if (request.MaxUses.HasValue)
        {
            coupon.MaxUses = request.MaxUses.Value;
        }

        if (request.StartDate.HasValue)
        {
            coupon.StartDate = request.StartDate.Value;
        }

        if (request.EndDate.HasValue)
        {
            coupon.EndDate = request.EndDate.Value;
        }

        if (request.IsActive.HasValue)
        {
            coupon.IsActive = request.IsActive.Value;
        }

        coupon.UpdatedAt = DateTime.UtcNow;
        coupon.UpdatedBy = userEmail;

        await _context.SaveChangesAsync();

        return Ok(coupon);
    }

    /// <summary>
    /// Update coupon status (toggle active/inactive)
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateCouponStatus(int id, [FromBody] UpdateCouponStatusRequest request)
    {
        var coupon = await _context.Coupons.FindAsync(id);

        if (coupon == null)
        {
            return NotFound(new { message = "Không tìm thấy mã giảm giá" });
        }

        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";

        coupon.IsActive = request.IsActive;
        coupon.UpdatedAt = DateTime.UtcNow;
        coupon.UpdatedBy = userEmail;

        await _context.SaveChangesAsync();

        return Ok(coupon);
    }

    /// <summary>
    /// Delete a coupon
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCoupon(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);

        if (coupon == null)
        {
            return NotFound(new { message = "Không tìm thấy mã giảm giá" });
        }

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// Request DTOs
public class CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "percent";
    public decimal Value { get; set; }
    public decimal? MaxDiscount { get; set; }
    public int? MaxUses { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCouponRequest
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public decimal? Value { get; set; }
    public decimal? MaxDiscount { get; set; }
    public int? MaxUses { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateCouponStatusRequest
{
    public bool IsActive { get; set; }
}

