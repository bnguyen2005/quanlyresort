using QuanLyResort.Repositories;
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
    private readonly IUnitOfWork _unitOfWork;
    private ResortDbContext _context => _unitOfWork.Context;

    public FAQsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// L?y danh sách FAQ (public - không c?n dang nh?p)
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
            return StatusCode(500, new { message = "L?i khi t?i FAQ", error = ex.Message });
        }
    }

    /// <summary>
    /// L?y FAQ theo ID (public)
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
                return NotFound(new { message = "FAQ không t?n t?i" });
            }

            // Tang view count
            var faqEntity = await _context.FAQs.FindAsync(id);
            if (faqEntity != null)
            {
                faqEntity.ViewCount++;
                await _unitOfWork.SaveChangesAsync();
            }

            return Ok(faq);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAQsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "L?i khi t?i FAQ", error = ex.Message });
        }
    }

    /// <summary>
    /// Ðánh giá FAQ h?u ích (public)
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
                return NotFound(new { message = "FAQ không t?n t?i" });
            }

            faq.HelpfulCount++;
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "C?m on ph?n h?i c?a b?n!", helpfulCount = faq.HelpfulCount });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAQsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "L?i khi c?p nh?t", error = ex.Message });
        }
    }

    /// <summary>
    /// L?y danh sách categories (public)
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
            return StatusCode(500, new { message = "L?i khi t?i danh m?c", error = ex.Message });
        }
    }

    // ========== ADMIN ENDPOINTS ==========

    /// <summary>
    /// T?o FAQ m?i (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateFAQ([FromBody] CreateFAQRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Question) || string.IsNullOrWhiteSpace(request.Answer))
            {
                return BadRequest(new { message = "Câu h?i và câu tr? l?i không du?c d? tr?ng" });
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
            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                message = "FAQ dã du?c t?o thành công",
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
            return StatusCode(500, new { message = "L?i khi t?o FAQ", error = ex.Message });
        }
    }

    /// <summary>
    /// C?p nh?t FAQ (Admin only)
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
                return NotFound(new { message = "FAQ không t?n t?i" });
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
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "FAQ dã du?c c?p nh?t thành công" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAQsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "L?i khi c?p nh?t FAQ", error = ex.Message });
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
                return NotFound(new { message = "FAQ không t?n t?i" });
            }

            _context.FAQs.Remove(faq);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "FAQ dã du?c xóa thành công" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAQsController] Error: {ex.Message}");
            return StatusCode(500, new { message = "L?i khi xóa FAQ", error = ex.Message });
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


