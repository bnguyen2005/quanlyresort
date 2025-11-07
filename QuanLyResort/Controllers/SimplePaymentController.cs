using Microsoft.AspNetCore.Mvc;
using QuanLyResort.Services;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyResort.Controllers;

/// <summary>
/// Controller đơn giản cho thanh toán - tạo PayOs payment link và xử lý webhook
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SimplePaymentController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly PayOsService _payOsService;
    private readonly ILogger<SimplePaymentController> _logger;

    public SimplePaymentController(
        IBookingService bookingService,
        PayOsService payOsService,
        ILogger<SimplePaymentController> logger)
    {
        _bookingService = bookingService;
        _payOsService = payOsService;
        _logger = logger;
    }

    /// <summary>
    /// Webhook đơn giản - nhận từ PayOs/VietQR
    /// Hỗ trợ 2 format:
    /// 1. PayOs format: { "code": "00", "desc": "success", "success": true, "data": { "orderCode": 123, "amount": 3000, "description": "BOOKING7", ... }, "signature": "..." }
    /// 2. Simple format: { "content": "BOOKING7", "amount": 5000, "transactionId": "..." }
    /// </summary>
    [HttpPost("webhook")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> Webhook([FromBody] object? rawRequest = null)
    {
        var webhookId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Handle PayOs verification request (empty body)
            if (rawRequest == null)
            {
                _logger.LogInformation("🔍 [WEBHOOK-{WebhookId}] PayOs verification request (empty body)", webhookId);
                return Ok(new
                {
                    status = "active",
                    endpoint = "/api/simplepayment/webhook",
                    message = "Webhook endpoint is ready",
                    timestamp = DateTime.UtcNow
                });
            }
            
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            _logger.LogInformation("📥 [WEBHOOK-{WebhookId}] Webhook received at {Time}", webhookId, startTime);
            _logger.LogInformation("   Raw request: {RawRequest}", System.Text.Json.JsonSerializer.Serialize(rawRequest));
            _logger.LogInformation("   IP Address: {RemoteIp}", HttpContext.Connection.RemoteIpAddress?.ToString());
            _logger.LogInformation("   User-Agent: {UserAgent}", Request.Headers["User-Agent"].ToString());
            
            // Parse request - hỗ trợ cả PayOs format và Simple format
            string? content = null;
            decimal amount = 0;
            string? transactionId = null;
            int? orderCode = null;
            
            // Try PayOs format first
            var payOsRequest = System.Text.Json.JsonSerializer.Deserialize<PayOsWebhookRequest>(System.Text.Json.JsonSerializer.Serialize(rawRequest));
            if (payOsRequest != null && payOsRequest.Success && payOsRequest.Data != null)
            {
                // PayOs format
                _logger.LogInformation("📋 [WEBHOOK-{WebhookId}] Detected PayOs format", webhookId);
                content = payOsRequest.Data.Description; // PayOs gửi booking ID trong description
                amount = payOsRequest.Data.Amount;
                transactionId = payOsRequest.Data.Reference;
                orderCode = payOsRequest.Data.OrderCode;
                
                _logger.LogInformation("   PayOs - Code: {Code}, Desc: {Desc}", payOsRequest.Code, payOsRequest.Desc);
                _logger.LogInformation("   PayOs - OrderCode: {OrderCode}, Amount: {Amount:N0} VND", orderCode, amount);
                _logger.LogInformation("   PayOs - Description: {Description}", content);
                _logger.LogInformation("   PayOs - Reference: {Reference}", transactionId);
            }
            else
            {
                // Try Simple format
                var simpleRequest = System.Text.Json.JsonSerializer.Deserialize<SimpleWebhookRequest>(System.Text.Json.JsonSerializer.Serialize(rawRequest));
                if (simpleRequest != null && (!string.IsNullOrEmpty(simpleRequest.Content) || simpleRequest.Amount > 0))
                {
                    _logger.LogInformation("📋 [WEBHOOK-{WebhookId}] Detected Simple format", webhookId);
                    content = simpleRequest.Content;
                    amount = simpleRequest.Amount;
                    transactionId = simpleRequest.TransactionId;
                }
            }
            
            // If still no data, check if it's empty verification request
            if (string.IsNullOrEmpty(content) && amount == 0)
            {
                _logger.LogInformation("🔍 [WEBHOOK-{WebhookId}] PayOs verification request (empty data)", webhookId);
                return Ok(new
                {
                    status = "active",
                    endpoint = "/api/simplepayment/webhook",
                    message = "Webhook endpoint is ready",
                    timestamp = DateTime.UtcNow
                });
            }
            
            Console.WriteLine($"\n📥 [WEBHOOK-{webhookId}] Webhook received: {content} - {amount:N0} VND");

            // Parse booking ID từ content hoặc orderCode
            _logger.LogInformation("🔍 [WEBHOOK-{WebhookId}] Extracting booking ID...", webhookId);
            int? bookingId = null;
            
            // Try orderCode first (PayOs format)
            if (orderCode.HasValue && orderCode.Value > 0)
            {
                bookingId = orderCode.Value;
                _logger.LogInformation("✅ [WEBHOOK-{WebhookId}] Using orderCode as bookingId: {BookingId}", webhookId, bookingId);
            }
            // Try extract from content/description
            else if (!string.IsNullOrEmpty(content))
            {
                bookingId = ExtractBookingId(content);
            }
            
            if (!bookingId.HasValue)
            {
                _logger.LogWarning("⚠️ [WEBHOOK-{WebhookId}] Cannot extract booking ID. Content: {Content}, OrderCode: {OrderCode}", 
                    webhookId, content, orderCode);
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
            if (amount > 0 && estimatedAmount > 0)
            {
                 // Cho phép sai số 10% hoặc chấp nhận nếu amount >= expected
                var diff = Math.Abs(amount - estimatedAmount);
                var maxDiff = estimatedAmount * 0.1m;
                
                // Chấp nhận nếu:
                // 1. Amount >= estimatedAmount (thanh toán đủ hoặc nhiều hơn)
                // 2. Hoặc sai số <= 10%
                if (amount < estimatedAmount && diff > maxDiff)
                {
                    _logger.LogWarning("⚠️ Amount mismatch: Expected={Expected}, Received={Received}", 
                        estimatedAmount, amount);
                    return BadRequest(new { message = "Số tiền không khớp" });
                }
                
                _logger.LogInformation("✅ Amount verified: Expected={Expected}, Received={Received}, Diff={Diff}", 
                    estimatedAmount, amount, diff);
            }

            // Update booking status using ProcessOnlinePaymentAsync
            _logger.LogInformation("🔄 [WEBHOOK-{WebhookId}] Updating booking {BookingId} to Paid status...", webhookId, bookingId.Value);
            var performedBy = $"Webhook-{transactionId ?? webhookId}";
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
    /// Tạo PayOs payment link
    /// </summary>
    [HttpPost("create-link")]
    [Authorize]
    public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentLinkRequest request)
    {
        try
        {
            _logger.LogInformation("🔄 [CreateLink] Creating PayOs payment link for booking {BookingId}", request.BookingId);

            // Get booking
            var booking = await _bookingService.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
            {
                return NotFound(new { message = $"Booking {request.BookingId} không tồn tại" });
            }

            // Check if already paid
            if (booking.Status == "Paid")
            {
                return BadRequest(new { message = "Đặt phòng này đã được thanh toán" });
            }

            // Get amount
            var amount = booking.EstimatedTotalAmount ?? 0;
            if (amount <= 0)
            {
                return BadRequest(new { message = "Số tiền thanh toán không hợp lệ" });
            }

            // Use bookingId as orderCode (PayOs requirement)
            var orderCode = request.BookingId;
            var description = $"BOOKING{request.BookingId}"; // PayOs description
            
            // Get base URL from request
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var returnUrl = $"{baseUrl}/customer/my-bookings.html?payment=success&bookingId={request.BookingId}";
            var cancelUrl = $"{baseUrl}/customer/my-bookings.html?payment=cancelled&bookingId={request.BookingId}";

            // Create payment link via PayOs API
            var expiredAt = DateTime.UtcNow.AddHours(24); // Expire after 24 hours
            var paymentLink = await _payOsService.CreatePaymentLinkAsync(
                orderCode: orderCode,
                amount: amount,
                description: description,
                returnUrl: returnUrl,
                cancelUrl: cancelUrl,
                expiredAt: expiredAt
            );

            if (paymentLink == null || paymentLink.Data == null)
            {
                _logger.LogError("❌ [CreateLink] Failed to create PayOs payment link. Code: {Code}, Desc: {Desc}", 
                    paymentLink?.Code, paymentLink?.Desc);
                return StatusCode(500, new { 
                    message = $"Không thể tạo mã thanh toán. {paymentLink?.Desc ?? "Vui lòng thử lại."}",
                    code = paymentLink?.Code,
                    desc = paymentLink?.Desc
                });
            }

            _logger.LogInformation("✅ [CreateLink] Payment link created: PaymentLinkId={PaymentLinkId}", 
                paymentLink.Data.PaymentLinkId);

            return Ok(new
            {
                success = true,
                paymentLinkId = paymentLink.Data.PaymentLinkId,
                orderCode = paymentLink.Data.OrderCode,
                qrCode = paymentLink.Data.QrCode, // Base64 QR code image
                checkoutUrl = paymentLink.Data.CheckoutUrl,
                amount = paymentLink.Data.Amount,
                description = paymentLink.Data.Description,
                accountNumber = paymentLink.Data.AccountNumber,
                accountName = paymentLink.Data.AccountName,
                expiredAt = paymentLink.Data.ExpiredAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [CreateLink] Error creating payment link: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "❌ [CreateLink] Inner exception: {Message}", ex.InnerException.Message);
            }
            return StatusCode(500, new { 
                message = "Lỗi tạo mã thanh toán", 
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
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
/// Request model cho webhook đơn giản (Simple format)
/// </summary>
public class SimpleWebhookRequest
{
    public string Content { get; set; } = string.Empty; // Nội dung chuyển khoản: "BOOKING-39"
    public decimal Amount { get; set; } // Số tiền
    public string? TransactionId { get; set; } // Mã giao dịch (optional)
}

/// <summary>
/// Request model cho PayOs webhook (PayOs format)
/// Format từ PayOs API documentation
/// </summary>
public class PayOsWebhookRequest
{
    public string Code { get; set; } = string.Empty; // "00" = success
    public string Desc { get; set; } = string.Empty; // "success"
    public bool Success { get; set; }
    public PayOsWebhookData? Data { get; set; }
    public string? Signature { get; set; }
}

/// <summary>
/// Data trong PayOs webhook
/// </summary>
public class PayOsWebhookData
{
    public int? OrderCode { get; set; } // Order code (có thể dùng làm bookingId)
    public decimal Amount { get; set; } // Số tiền
    public string? Description { get; set; } // Mô tả (có thể chứa booking ID: "BOOKING7")
    public string? AccountNumber { get; set; }
    public string? Reference { get; set; } // Mã tham chiếu giao dịch
    public string? TransactionDateTime { get; set; }
    public string? Currency { get; set; }
    public string? PaymentLinkId { get; set; }
}

/// <summary>
/// Request để tạo PayOs payment link
/// </summary>
public class CreatePaymentLinkRequest
{
    public int BookingId { get; set; }
}

