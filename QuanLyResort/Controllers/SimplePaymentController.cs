using Microsoft.AspNetCore.Mvc;
using QuanLyResort.Services;
using System.Text.RegularExpressions;

namespace QuanLyResort.Controllers;

/// <summary>
/// Controller đơn giản cho thanh toán - chỉ xử lý webhook
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SimplePaymentController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<SimplePaymentController> _logger;

    public SimplePaymentController(
        IBookingService bookingService,
        ILogger<SimplePaymentController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// Webhook đơn giản - nhận từ PayOs/VietQR
    /// </summary>
    [HttpPost("webhook")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> Webhook([FromBody] SimpleWebhookRequest request)
    {
        var webhookId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            _logger.LogInformation("📥 [WEBHOOK-{WebhookId}] Webhook received at {Time}", webhookId, startTime);
            _logger.LogInformation("   Content: {Content}", request.Content);
            _logger.LogInformation("   Amount: {Amount:N0} VND", request.Amount);
            _logger.LogInformation("   TransactionId: {TransactionId}", request.TransactionId ?? "N/A");
            _logger.LogInformation("   IP Address: {RemoteIp}", HttpContext.Connection.RemoteIpAddress?.ToString());
            _logger.LogInformation("   User-Agent: {UserAgent}", Request.Headers["User-Agent"].ToString());
            
            Console.WriteLine($"\n📥 [WEBHOOK-{webhookId}] Webhook received: {request.Content} - {request.Amount:N0} VND");

            // Parse booking ID từ content
            _logger.LogInformation("🔍 [WEBHOOK-{WebhookId}] Extracting booking ID from content...", webhookId);
            var bookingId = ExtractBookingId(request.Content);
            if (!bookingId.HasValue)
            {
                _logger.LogWarning("⚠️ [WEBHOOK-{WebhookId}] Cannot extract booking ID from content: {Content}", webhookId, request.Content);
                Console.WriteLine($"⚠️ [WEBHOOK-{webhookId}] Failed: Cannot extract booking ID");
                return BadRequest(new { message = "Không tìm thấy booking ID trong nội dung", webhookId });
            }
            _logger.LogInformation("✅ [WEBHOOK-{WebhookId}] Extracted booking ID: {BookingId}", webhookId, bookingId.Value);
            Console.WriteLine($"✅ [WEBHOOK-{webhookId}] Booking ID: {bookingId.Value}");

            // Get booking
            _logger.LogInformation("🔍 [WEBHOOK-{WebhookId}] Fetching booking {BookingId}...", webhookId, bookingId.Value);
            var booking = await _bookingService.GetBookingByIdAsync(bookingId.Value);
            if (booking == null)
            {
                _logger.LogWarning("⚠️ [WEBHOOK-{WebhookId}] Booking {BookingId} not found", webhookId, bookingId.Value);
                Console.WriteLine($"❌ [WEBHOOK-{webhookId}] Booking {bookingId.Value} not found");
                return NotFound(new { message = $"Booking {bookingId.Value} không tồn tại", webhookId });
            }

            _logger.LogInformation("✅ [WEBHOOK-{WebhookId}] Booking found: Code={BookingCode}, Status={Status}, Amount={Amount:N0} VND", 
                webhookId, booking.BookingCode, booking.Status, booking.EstimatedTotalAmount ?? 0);
            Console.WriteLine($"✅ [WEBHOOK-{webhookId}] Booking {booking.BookingCode} - Status: {booking.Status} - Amount: {booking.EstimatedTotalAmount:N0} VND");

            // Check if already paid
            if (booking.Status == "Paid")
            {
                _logger.LogInformation("✅ [WEBHOOK-{WebhookId}] Booking {BookingId} already paid, ignoring duplicate", webhookId, bookingId.Value);
                Console.WriteLine($"ℹ️ [WEBHOOK-{webhookId}] Booking already paid - ignoring");
                return Ok(new { message = "Đã thanh toán rồi", bookingId = bookingId.Value, webhookId });
            }

            // Verify amount (optional - có thể bỏ qua nếu muốn đơn giản hơn)
            var estimatedAmount = booking.EstimatedTotalAmount ?? 0;
            if (request.Amount > 0 && estimatedAmount > 0)
            {
                 // Cho phép sai số 10% hoặc chấp nhận nếu amount >= expected
                var diff = Math.Abs(request.Amount - estimatedAmount);
                var maxDiff = estimatedAmount * 0.1m;
                
                // Chấp nhận nếu:
                // 1. Amount >= estimatedAmount (thanh toán đủ hoặc nhiều hơn)
                // 2. Hoặc sai số <= 10%
                if (request.Amount < estimatedAmount && diff > maxDiff)
                {
                    _logger.LogWarning("⚠️ Amount mismatch: Expected={Expected}, Received={Received}", 
                        estimatedAmount, request.Amount);
                    return BadRequest(new { message = "Số tiền không khớp" });
                }
                
                _logger.LogInformation("✅ Amount verified: Expected={Expected}, Received={Received}, Diff={Diff}", 
                    estimatedAmount, request.Amount, diff);
            }

            // Update booking status using ProcessOnlinePaymentAsync
            _logger.LogInformation("🔄 [WEBHOOK-{WebhookId}] Updating booking {BookingId} to Paid status...", webhookId, bookingId.Value);
            var performedBy = $"Webhook-{request.TransactionId ?? webhookId}";
            var updated = await _bookingService.ProcessOnlinePaymentAsync(bookingId.Value, performedBy);
            if (!updated)
            {
                _logger.LogError("❌ [WEBHOOK-{WebhookId}] Failed to update booking {BookingId}", webhookId, bookingId.Value);
                Console.WriteLine($"❌ [WEBHOOK-{webhookId}] Failed to update booking");
                return StatusCode(500, new { message = "Không thể cập nhật booking", webhookId });
            }

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("✅ [WEBHOOK-{WebhookId}] Booking {BookingId} ({BookingCode}) updated to Paid successfully!", 
                webhookId, bookingId.Value, booking.BookingCode);
            _logger.LogInformation("⏱️ [WEBHOOK-{WebhookId}] Processing time: {Duration}ms", webhookId, duration);
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            
            Console.WriteLine($"✅ [WEBHOOK-{webhookId}] SUCCESS! Booking {booking.BookingCode} updated to Paid ({duration:F0}ms)");

            return Ok(new
            {
                success = true,
                message = "Thanh toán thành công",
                bookingId = bookingId.Value,
                bookingCode = booking.BookingCode,
                webhookId = webhookId,
                processedAt = DateTime.UtcNow,
                durationMs = duration
            });
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "❌ [WEBHOOK-{WebhookId}] Error processing webhook after {Duration}ms", webhookId, duration);
            Console.WriteLine($"❌ [WEBHOOK-{webhookId}] ERROR: {ex.Message}");
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            return StatusCode(500, new { message = "Lỗi xử lý webhook", error = ex.Message, webhookId });
        }
    }

    /// <summary>
    /// Endpoint để PayOs verify webhook URL (GET request)
    /// PayOs sẽ gửi GET request để verify webhook URL trước khi chấp nhận
    /// </summary>
    [HttpGet("webhook")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public IActionResult VerifyWebhook()
    {
        _logger.LogInformation("🔍 [WEBHOOK-VERIFY] PayOs verification request received");
        return Ok(new
        {
            status = "active",
            endpoint = "/api/simplepayment/webhook",
            message = "Webhook endpoint is ready",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Endpoint để kiểm tra trạng thái webhook system
    /// </summary>
    [HttpGet("webhook-status")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public IActionResult GetWebhookStatus()
    {
        return Ok(new
        {
            status = "active",
            endpoint = "/api/simplepayment/webhook",
            timestamp = DateTime.UtcNow,
            supportedFormats = new[]
            {
                "BOOKING-{id}",
                "BOOKING-BKG{id}",
                "{id} (direct booking ID)"
            },
            message = "Webhook system is ready to receive payments"
        });
    }

    /// <summary>
    /// Extract booking ID từ content
    /// Format: "BOOKING-39", "BOOKING7", "BOOKING-BKG2025039", hoặc chỉ số "7"
    /// </summary>
    private int? ExtractBookingId(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        // Normalize content: uppercase và trim
        var normalizedContent = content.ToUpper().Trim();

        // Pattern 1: "BOOKING-39" hoặc "BOOKING_39" (có dấu gạch ngang/gạch dưới)
        var pattern1 = @"BOOKING[-_](\d+)";
        var match1 = Regex.Match(normalizedContent, pattern1, RegexOptions.IgnoreCase);
        if (match1.Success && match1.Groups.Count > 1)
        {
            if (int.TryParse(match1.Groups[1].Value, out var id))
                return id;
        }

        // Pattern 2: "BOOKING7" hoặc "BOOKING39" (KHÔNG có dấu gạch ngang) - QUAN TRỌNG!
        var pattern2 = @"BOOKING(\d+)";
        var match2 = Regex.Match(normalizedContent, pattern2, RegexOptions.IgnoreCase);
        if (match2.Success && match2.Groups.Count > 1)
        {
            if (int.TryParse(match2.Groups[1].Value, out var id))
            {
                _logger.LogInformation("✅ Extracted booking ID from pattern 'BOOKING{Id}': {BookingId}", id, id);
                return id;
            }
        }

        // Pattern 3: "BOOKING-BKG2025039" -> extract "39" từ cuối
        var pattern3 = @"BOOKING[-_]?BKG\d+(\d{1,3})";
        var match3 = Regex.Match(normalizedContent, pattern3, RegexOptions.IgnoreCase);
        if (match3.Success && match3.Groups.Count > 1)
        {
            if (int.TryParse(match3.Groups[1].Value, out var id))
                return id;
        }

        // Pattern 4: Chỉ số (nếu hợp lý: 1-9999)
        if (int.TryParse(normalizedContent, out var directId) && directId > 0 && directId < 10000)
            return directId;

        return null;
    }
}

/// <summary>
/// Request model cho webhook đơn giản
/// </summary>
public class SimpleWebhookRequest
{
    public string Content { get; set; } = string.Empty; // Nội dung chuyển khoản: "BOOKING-39"
    public decimal Amount { get; set; } // Số tiền
    public string? TransactionId { get; set; } // Mã giao dịch (optional)
}

