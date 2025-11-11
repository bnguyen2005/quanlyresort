using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(ResortDbContext context, ILogger<ReviewsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả đánh giá (công khai)
    /// GET /api/reviews?roomId=1&rating=5&limit=10
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllReviews(
        [FromQuery] int? roomId = null,
        [FromQuery] int? rating = null,
        [FromQuery] int limit = 50)
    {
        var query = _context.Reviews
            .Include(r => r.Customer)
            .Include(r => r.Room)
            .Where(r => r.IsVisible && r.IsApproved)
            .AsQueryable();

        if (roomId.HasValue)
        {
            query = query.Where(r => r.RoomId == roomId.Value);
        }

        if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
        {
            query = query.Where(r => r.Rating == rating.Value);
        }

        var reviewsData = await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .Select(r => new
            {
                r.ReviewId,
                r.Rating,
                r.Comment,
                r.Response,
                r.ResponseDate,
                r.RespondedBy,
                r.CreatedAt,
                CustomerName = r.Customer != null ? 
                    (r.Customer.FullName ?? "Khách hàng") : 
                    "Khách hàng",
                CustomerFullName = r.Customer != null ? r.Customer.FullName : null,
                RoomNumber = r.Room != null ? r.Room.RoomNumber : null,
                RoomType = r.Room != null ? r.Room.RoomType : null
            })
            .ToListAsync();
        
        // Calculate initials in memory (SQLite doesn't support custom methods in Select)
        var reviews = reviewsData.Select(r => new
        {
            r.ReviewId,
            r.Rating,
            r.Comment,
            r.Response,
            r.ResponseDate,
            r.RespondedBy,
            r.CreatedAt,
            r.CustomerName,
            CustomerInitials = GetInitials(r.CustomerFullName),
            r.RoomNumber,
            r.RoomType
        }).ToList();

        // Calculate statistics
        var stats = await _context.Reviews
            .Where(r => r.IsVisible && r.IsApproved && (!roomId.HasValue || r.RoomId == roomId.Value))
            .GroupBy(r => r.Rating)
            .Select(g => new
            {
                Rating = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        var totalReviews = stats.Sum(s => s.Count);
        
        // Calculate average rating in memory (SQLite limitation)
        var averageRating = 0.0;
        if (totalReviews > 0)
        {
            var allRatings = await _context.Reviews
                .Where(r => r.IsVisible && r.IsApproved && (!roomId.HasValue || r.RoomId == roomId.Value))
                .Select(r => r.Rating)
                .ToListAsync();
            
            averageRating = allRatings.Any() ? allRatings.Average(r => (double)r) : 0;
        }

        return Ok(new
        {
            reviews,
            statistics = new
            {
                totalReviews,
                averageRating = Math.Round(averageRating, 1),
                ratingDistribution = stats
            }
        });
    }

    /// <summary>
    /// Lấy đánh giá theo ID
    /// GET /api/reviews/{id}
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviewById(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.Customer)
            .Include(r => r.Room)
            .Where(r => r.ReviewId == id && r.IsVisible && r.IsApproved)
            .Select(r => new
            {
                r.ReviewId,
                r.Rating,
                r.Comment,
                r.Response,
                r.ResponseDate,
                r.RespondedBy,
                r.CreatedAt,
                CustomerName = r.Customer != null ? 
                    (r.Customer.FullName ?? "Khách hàng") : 
                    "Khách hàng",
                RoomNumber = r.Room != null ? r.Room.RoomNumber : null
            })
            .FirstOrDefaultAsync();

        if (review == null)
        {
            return NotFound(new { message = "Review not found." });
        }

        return Ok(review);
    }

    /// <summary>
    /// Tạo đánh giá mới (yêu cầu đăng nhập)
    /// POST /api/reviews
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var customerIdClaim = HttpContext.User.FindFirst("CustomerId")?.Value;
        if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
        {
            return Unauthorized(new { message = "Customer ID not found in token." });
        }

        // Validate customer exists
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
        {
            return NotFound(new { message = "Customer not found." });
        }

        // Validate room if provided
        if (request.RoomId.HasValue)
        {
            var room = await _context.Rooms.FindAsync(request.RoomId.Value);
            if (room == null)
            {
                return BadRequest(new { message = "Room not found." });
            }

            // Allow review even if customer hasn't booked - anyone can review
            // Check if customer already reviewed this room (prevent duplicate)
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.CustomerId == customerId 
                    && r.RoomId == request.RoomId.Value);
            
            if (existingReview != null)
            {
                return BadRequest(new { message = "Bạn đã đánh giá phòng này rồi. Bạn có thể chỉnh sửa đánh giá của mình." });
            }
        }

        var review = new Review
        {
            CustomerId = customerId,
            RoomId = request.RoomId,
            BookingId = request.BookingId,
            Rating = request.Rating,
            Comment = request.Comment,
            IsApproved = true,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReviewById), new { id = review.ReviewId }, new
        {
            review.ReviewId,
            review.Rating,
            review.Comment,
            review.CreatedAt,
            message = "Đánh giá của bạn đã được gửi thành công!"
        });
    }

    /// <summary>
    /// Admin phản hồi đánh giá
    /// PUT /api/reviews/{id}/response
    /// </summary>
    [HttpPut("{id}/response")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RespondToReview(int id, [FromBody] ReviewResponseRequest request)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound(new { message = "Review not found." });
        }

        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value 
            ?? HttpContext.User.FindFirst("Username")?.Value 
            ?? "Admin";

        review.Response = request.Response;
        review.ResponseDate = DateTime.UtcNow;
        review.RespondedBy = username;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Response added successfully.", review });
    }

    /// <summary>
    /// Xóa đánh giá (Admin only)
    /// DELETE /api/reviews/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound(new { message = "Review not found." });
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Review deleted successfully." });
    }

    /// <summary>
    /// Kiểm tra customer đã từng check-out phòng này chưa
    /// GET /api/reviews/can-review/{roomId}
    /// </summary>
    [HttpGet("can-review/{roomId}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CanReviewRoom(int roomId)
    {
        var customerIdClaim = HttpContext.User.FindFirst("CustomerId")?.Value;
        if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
        {
            return Unauthorized(new { message = "Customer ID not found in token." });
        }

        // Allow anyone to review - no booking required
        // Check if already reviewed
        var existingReview = await _context.Reviews
            .Where(r => r.CustomerId == customerId && r.RoomId == roomId)
            .Select(r => new { r.ReviewId, r.Rating, r.Comment, r.CreatedAt })
            .FirstOrDefaultAsync();

        return Ok(new
        {
            canReview = existingReview == null,
            hasUsedRoom = true, // Always true now - no booking required
            existingReview
        });
    }

    /// <summary>
    /// Lấy danh sách phòng customer đã check-out (có thể đánh giá)
    /// GET /api/reviews/reviewable-rooms
    /// </summary>
    [HttpGet("reviewable-rooms")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetReviewableRooms()
    {
        var customerIdClaim = HttpContext.User.FindFirst("CustomerId")?.Value;
        if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
        {
            return Unauthorized(new { message = "Customer ID not found in token." });
        }

        // Get rooms customer has used (can review)
        // Include: CheckedOut, Paid and past checkout, Confirmed and past checkout
        var usedRooms = await _context.Bookings
            .Include(b => b.Room)
            .ThenInclude(r => r.RoomTypeNavigation)
            .Where(b => b.CustomerId == customerId 
                && b.RoomId.HasValue
                && (
                    b.Status == "CheckedOut" ||
                    (b.Status == "Paid" && b.CheckOutDate <= DateTime.UtcNow) ||
                    (b.Status == "Confirmed" && b.CheckOutDate <= DateTime.UtcNow.AddDays(-1))
                ))
            .Select(b => new
            {
                RoomId = b.RoomId.Value,
                RoomNumber = b.Room!.RoomNumber,
                RoomType = b.Room.RoomType,
                RoomTypeName = b.Room.RoomTypeNavigation != null ? b.Room.RoomTypeNavigation.TypeName : null,
                CheckOutDate = b.ActualCheckOutTime ?? b.CheckOutDate,
                BookingId = b.BookingId,
                BookingCode = b.BookingCode
            })
            .ToListAsync();

        // Get already reviewed room IDs
        var reviewedRoomIds = await _context.Reviews
            .Where(r => r.CustomerId == customerId && r.RoomId.HasValue)
            .Select(r => r.RoomId!.Value)
            .ToListAsync();

        // Filter out already reviewed rooms
        var reviewableRooms = usedRooms
            .Where(r => !reviewedRoomIds.Contains(r.RoomId))
            .GroupBy(r => r.RoomId)
            .Select(g => g.OrderByDescending(r => r.CheckOutDate).First())
            .ToList();

        return Ok(reviewableRooms);
    }

    /// <summary>
    /// Lấy tất cả đánh giá cho admin (bao gồm cả chưa approved)
    /// GET /api/reviews/admin?status=all&roomId=1&rating=5
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAllReviewsForAdmin(
        [FromQuery] string? status = "all", // all, approved, pending, hidden
        [FromQuery] int? roomId = null,
        [FromQuery] int? rating = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        const string logPrefix = "[ReviewsController.GetAllReviewsForAdmin]";
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        Console.WriteLine($"{logPrefix} [{timestamp}] ========== START ==========");
        Console.WriteLine($"{logPrefix} [{timestamp}] Request params: status={status}, roomId={roomId}, rating={rating}, search={search}, fromDate={fromDate}, toDate={toDate}");
        
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            Console.WriteLine($"{logPrefix} [{timestamp}] User: {userEmail}, Role: {userRole}");
            
            var query = _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .AsQueryable();
            
            Console.WriteLine($"{logPrefix} [{timestamp}] Initial query count: {await query.CountAsync()}");

        // Filter by status
        if (status == "approved")
        {
            query = query.Where(r => r.IsApproved && r.IsVisible);
        }
        else if (status == "pending")
        {
            query = query.Where(r => !r.IsApproved);
        }
        else if (status == "hidden")
        {
            query = query.Where(r => !r.IsVisible);
        }
        // "all" - no filter

        if (roomId.HasValue)
        {
            query = query.Where(r => r.RoomId == roomId.Value);
        }

        if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
        {
            query = query.Where(r => r.Rating == rating.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(r => 
                (r.Customer != null && r.Customer.FullName != null && r.Customer.FullName.Contains(search)) ||
                (r.Comment != null && r.Comment.Contains(search)) ||
                (r.Room != null && r.Room.RoomNumber != null && r.Room.RoomNumber.Contains(search)));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= toDate.Value);
        }

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.ReviewId,
                    r.Rating,
                    r.Comment,
                    r.Response,
                    r.ResponseDate,
                    r.RespondedBy,
                    r.IsApproved,
                    r.IsVisible,
                    r.CreatedAt,
                    r.UpdatedAt,
                    CustomerName = r.Customer != null ? 
                        (r.Customer.FullName ?? "Khách hàng") : 
                        "Khách hàng",
                    CustomerEmail = r.Customer != null ? r.Customer.Email : null,
                    RoomNumber = r.Room != null ? r.Room.RoomNumber : null,
                    RoomType = r.Room != null ? r.Room.RoomType : null
                })
                .ToListAsync();

            Console.WriteLine($"{logPrefix} [{timestamp}] ✅ Found {reviews.Count} reviews");
            Console.WriteLine($"{logPrefix} [{timestamp}] ========== END (SUCCESS) ==========");
            
            return Ok(new
            {
                reviews,
                total = reviews.Count
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{logPrefix} [{timestamp}] ❌ ========== ERROR ==========");
            Console.WriteLine($"{logPrefix} [{timestamp}] ❌ Error message: {ex.Message}");
            Console.WriteLine($"{logPrefix} [{timestamp}] ❌ Stack trace: {ex.StackTrace}");
            Console.WriteLine($"{logPrefix} [{timestamp}] ❌ Inner exception: {ex.InnerException?.Message}");
            Console.WriteLine($"{logPrefix} [{timestamp}] ========== END (ERROR) ==========");
            return StatusCode(500, new { message = "Lỗi khi tải đánh giá", error = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật trạng thái đánh giá (approve/hide/delete)
    /// PUT /api/reviews/{id}/status
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateReviewStatus(int id, [FromBody] UpdateReviewStatusRequest request)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound(new { message = "Review not found." });
        }

        if (request.IsApproved.HasValue)
        {
            review.IsApproved = request.IsApproved.Value;
        }

        if (request.IsVisible.HasValue)
        {
            review.IsVisible = request.IsVisible.Value;
        }

        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Review status updated successfully.", review });
    }

    /// <summary>
    /// Helper method to get initials from full name
    /// </summary>
    private static string GetInitials(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "K";

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }
        else if (parts.Length == 1)
        {
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
        }
        
        return "K";
    }
}

public class CreateReviewRequest
{
    public int? RoomId { get; set; }
    public int? BookingId { get; set; }
    
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Comment { get; set; } = string.Empty;
}

public class ReviewResponseRequest
{
    [Required]
    [StringLength(500)]
    public string Response { get; set; } = string.Empty;
}

public class UpdateReviewStatusRequest
{
    public bool? IsApproved { get; set; }
    public bool? IsVisible { get; set; }
}

