using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FAQsController : ControllerBase
{
    private readonly ResortDbContext _context;

    public FAQsController(ResortDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách FAQ (public - không cần đăng nhập)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetFAQs([FromQuery] string? category = null, [FromQuery] string? search = null)
    {
        try
        {
            var query = _context.FAQs.Where(f => f.IsActive);

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(f => f.Category == category);
            }

            // Search in question and answer
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(f => 
                    f.Question.ToLower().Contains(searchLower) || 
                    f.Answer.ToLower().Contains(searchLower));
            }

            var faqs = await query
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.Question)
                .Select(f => new
                {
                    f.FAQId,
                    f.Question,
                    f.Answer,
                    f.Category,
                    f.DisplayOrder,
                    f.ViewCount,
                    f.HelpfulCount
                })
                .ToListAsync();

            return Ok(faqs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAQsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi tải FAQ", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy FAQ theo ID (public)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFAQ(int id)
    {
        try
        {
            var faq = await _context.FAQs
                .Where(f => f.FAQId == id && f.IsActive)
                .Select(f => new
                {
                    f.FAQId,
                    f.Question,
                    f.Answer,
                    f.Category,
                    f.DisplayOrder,
                    f.ViewCount,
                    f.HelpfulCount
                })
                .FirstOrDefaultAsync();

            if (faq == null)
            {
                return NotFound(new { message = "FAQ không tồn tại" });
            }

            // Tăng view count
            var faqEntity = await _context.FAQs.FindAsync(id);
            if (faqEntity != null)
            {
                faqEntity.ViewCount++;
                await _context.SaveChangesAsync();
            }

            return Ok(faq);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAQsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi tải FAQ", error = ex.Message });
        }
    }

    /// <summary>
    /// Đánh giá FAQ hữu ích (public)
    /// </summary>
    [HttpPost("{id}/helpful")]
    [AllowAnonymous]
    public async Task<IActionResult> MarkHelpful(int id)
    {
        try
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null || !faq.IsActive)
            {
                return NotFound(new { message = "FAQ không tồn tại" });
            }

            faq.HelpfulCount++;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cảm ơn phản hồi của bạn!", helpfulCount = faq.HelpfulCount });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAQsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi cập nhật", error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách categories (public)
    /// </summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _context.FAQs
                .Where(f => f.IsActive)
                .Select(f => f.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAQsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi tải danh mục", error = ex.Message });
        }
    }

    // ========== ADMIN ENDPOINTS ==========

    /// <summary>
    /// Tạo FAQ mới (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateFAQ([FromBody] CreateFAQRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Question) || string.IsNullOrWhiteSpace(request.Answer))
            {
                return BadRequest(new { message = "Câu hỏi và câu trả lời không được để trống" });
            }

            var faq = new FAQ
            {
                Question = request.Question.Trim(),
                Answer = request.Answer.Trim(),
                Category = request.Category ?? "General",
                DisplayOrder = request.DisplayOrder ?? 0,
                IsActive = true,
                CreatedBy = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "System",
                CreatedAt = DateTime.UtcNow
            };

            _context.FAQs.Add(faq);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "FAQ đã được tạo thành công",
                faq = new
                {
                    faq.FAQId,
                    faq.Question,
                    faq.Answer,
                    faq.Category,
                    faq.DisplayOrder
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAQsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi tạo FAQ", error = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật FAQ (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateFAQ(int id, [FromBody] UpdateFAQRequest request)
    {
        try
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null)
            {
                return NotFound(new { message = "FAQ không tồn tại" });
            }

            if (!string.IsNullOrWhiteSpace(request.Question))
                faq.Question = request.Question.Trim();

            if (!string.IsNullOrWhiteSpace(request.Answer))
                faq.Answer = request.Answer.Trim();

            if (!string.IsNullOrWhiteSpace(request.Category))
                faq.Category = request.Category;

            if (request.DisplayOrder.HasValue)
                faq.DisplayOrder = request.DisplayOrder.Value;

            if (request.IsActive.HasValue)
                faq.IsActive = request.IsActive.Value;

            faq.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "FAQ đã được cập nhật thành công" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAQsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi cập nhật FAQ", error = ex.Message });
        }
    }

    /// <summary>
    /// Xóa FAQ (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteFAQ(int id)
    {
        try
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null)
            {
                return NotFound(new { message = "FAQ không tồn tại" });
            }

            _context.FAQs.Remove(faq);
            await _context.SaveChangesAsync();

            return Ok(new { message = "FAQ đã được xóa thành công" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAQsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi khi xóa FAQ", error = ex.Message });
        }
    }
}

// DTOs
public class CreateFAQRequest
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int? DisplayOrder { get; set; }
}

public class UpdateFAQRequest
{
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public string? Category { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
}

