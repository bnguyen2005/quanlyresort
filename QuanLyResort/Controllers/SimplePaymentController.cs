using Microsoft.AspNetCore.Mvc;
using QuanLyResort.Services;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json.Serialization;
using QuanLyResort.Data;
using Microsoft.EntityFrameworkCore;

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
    private readonly ResortDbContext _context;

    public SimplePaymentController(
        IBookingService bookingService,
        PayOsService payOsService,
        ILogger<SimplePaymentController> logger,
        ResortDbContext context)
    {
        _bookingService = bookingService;
        _payOsService = payOsService;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Webhook đơn giản - nhận từ PayOs/VietQR
    /// Hỗ trợ 2 format:
    /// 1. PayOs format: { "code": "00", "desc": "success", "success": true, "data": { "orderCode": 123, "amount": 3000, "description": "BOOKING7", ... }, "signature": "..." }
    /// 2. Simple format: { "content": "BOOKING7", "amount": 5000, "transactionId": "..." }
    /// </summary>
    [HttpPost("webhook")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var webhookId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Read raw request body
            string rawRequestJson;
            using (var reader = new StreamReader(Request.Body))
            {
                rawRequestJson = await reader.ReadToEndAsync();
            }
            
            // Handle PayOs verification request (empty body)
            if (string.IsNullOrWhiteSpace(rawRequestJson))
            {
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] PayOs verification request (empty body)", webhookId);
                return Ok(new
                {
                    status = "active",
                    endpoint = "/api/simplepayment/webhook",
                    message = "Webhook endpoint is ready",
                    timestamp = DateTime.UtcNow
                });
            }
            
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            _logger.LogInformation("[WEBHOOK] 📥 [WEBHOOK-{WebhookId}] Webhook received at {Time}", webhookId, startTime);
            _logger.LogInformation("[WEBHOOK]    Raw request JSON: {RawRequest}", rawRequestJson);
            _logger.LogInformation("[WEBHOOK]    IP Address: {RemoteIp}", HttpContext.Connection.RemoteIpAddress?.ToString());
            _logger.LogInformation("[WEBHOOK]    User-Agent: {UserAgent}", Request.Headers["User-Agent"].ToString());
            
            // Parse request - hỗ trợ cả PayOs format và Simple format
            string? content = null;
            decimal amount = 0;
            string? transactionId = null;
            long? orderCode = null;
            
            // Try PayOs format first
            PayOsWebhookRequest? payOsRequest = null;
            try
            {
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Attempting to deserialize as PayOs format...", webhookId);
                // Cấu hình JsonSerializerOptions để case-insensitive và cho phép trailing commas
                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Quan trọng: cho phép match lowercase với PascalCase
                    AllowTrailingCommas = true,
                    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
                };
                payOsRequest = System.Text.Json.JsonSerializer.Deserialize<PayOsWebhookRequest>(rawRequestJson, jsonOptions);
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] PayOs deserialization result: Code={Code}, Desc={Desc}, Success={Success}, Data={HasData}", 
                    webhookId, payOsRequest?.Code ?? "NULL", payOsRequest?.Desc ?? "NULL", payOsRequest?.Success, payOsRequest?.Data != null);
                
                if (payOsRequest != null)
                {
                    _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] PayOs request details: Code='{Code}', Desc='{Desc}', Success={Success}, Data is null: {DataIsNull}", 
                        webhookId, payOsRequest.Code, payOsRequest.Desc, payOsRequest.Success, payOsRequest.Data == null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] Failed to deserialize as PayOs format: {Error}", webhookId, ex.Message);
                _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] Exception type: {ExceptionType}, Stack trace: {StackTrace}", 
                    webhookId, ex.GetType().Name, ex.StackTrace);
            }
            
            // PayOs gửi "code": "00" cho success, có thể có field "success": true
            // Check cả Code và Data để đảm bảo đúng format PayOs
            _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Checking PayOs format conditions: payOsRequest is null: {IsNull}, Code is empty: {CodeEmpty}, Data is null: {DataNull}", 
                webhookId, payOsRequest == null, string.IsNullOrEmpty(payOsRequest?.Code ?? ""), payOsRequest?.Data == null);
            
            if (payOsRequest != null)
            {
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] payOsRequest is NOT null, checking details...", webhookId);
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Code value: '{Code}' (IsEmpty: {IsEmpty})", 
                    webhookId, payOsRequest.Code ?? "NULL", string.IsNullOrEmpty(payOsRequest.Code));
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Data is null: {DataIsNull}", webhookId, payOsRequest.Data == null);
                
                if (!string.IsNullOrEmpty(payOsRequest.Code) && payOsRequest.Data != null)
                {
                    // PayOs format
                    _logger.LogInformation("[WEBHOOK] 📋 [WEBHOOK-{WebhookId}] ✅ Detected PayOs format - entering PayOs processing block", webhookId);
                    content = payOsRequest.Data.Description; // PayOs gửi booking ID trong description
                    amount = payOsRequest.Data.Amount;
                    transactionId = payOsRequest.Data.Reference;
                    orderCode = payOsRequest.Data.OrderCode;
                    
                    _logger.LogInformation("[WEBHOOK]    PayOs - Code: {Code}, Desc: {Desc}", payOsRequest.Code, payOsRequest.Desc);
                    _logger.LogInformation("[WEBHOOK]    PayOs - OrderCode: {OrderCode}, Amount: {Amount:N0} VND", orderCode, amount);
                    _logger.LogInformation("[WEBHOOK]    PayOs - Description: '{Description}'", content);
                    _logger.LogInformation("[WEBHOOK]    PayOs - Reference: {Reference}", transactionId);
                    _logger.LogInformation("[WEBHOOK]    PayOs - Extracted content: '{Content}', amount: {Amount}", content, amount);
                    
                    // Chỉ xử lý nếu code = "00" (success)
                    _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Checking PayOs code: '{Code}' == '00'? {IsSuccess}", 
                        webhookId, payOsRequest.Code, payOsRequest.Code == "00");
                    
                    if (payOsRequest.Code != "00")
                    {
                        _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] PayOs webhook failed with code: {Code}, Desc: {Desc}", 
                        webhookId, payOsRequest.Code, payOsRequest.Desc);
                        return Ok(new { message = $"Payment failed: {payOsRequest.Desc}", code = payOsRequest.Code });
                    }
                    
                    _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] PayOs code is '00' (success), continuing processing...", webhookId);
                }
                else
                {
                    _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] PayOs format check failed: Code empty={CodeEmpty}, Data null={DataNull}", 
                        webhookId, string.IsNullOrEmpty(payOsRequest.Code), payOsRequest.Data == null);
                }
            }
            else
            {
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] payOsRequest is NULL, will try Simple format", webhookId);
            }
            
            // Check if we successfully extracted PayOs data
            if (payOsRequest != null && !string.IsNullOrEmpty(payOsRequest.Code) && payOsRequest.Data != null && payOsRequest.Code == "00")
            {
                // PayOs format successfully processed - continue with extracted data
                _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] PayOs format successfully processed, extracted data: Content='{Content}', Amount={Amount}", 
                    webhookId, content, amount);
            }
            else
            {
                // Try Simple format
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] PayOs format not detected, trying Simple format...", webhookId);
                SimpleWebhookRequest? simpleRequest = null;
                try
                {
                    var jsonOptions = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true
                    };
                    simpleRequest = System.Text.Json.JsonSerializer.Deserialize<SimpleWebhookRequest>(rawRequestJson, jsonOptions);
                    _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Simple deserialization result: Content={Content}, Amount={Amount}, TransferAmount={TransferAmount}", 
                        webhookId, simpleRequest?.Content ?? "NULL", simpleRequest?.Amount ?? 0, simpleRequest?.TransferAmount?.ToString() ?? "NULL");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] Failed to deserialize as Simple format: {Error}", webhookId, ex.Message);
                }
                
                if (simpleRequest != null)
                {
                    _logger.LogInformation("[WEBHOOK] 📋 [WEBHOOK-{WebhookId}] Detected Simple/SePay format", webhookId);
                    _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Simple request fields: Content='{Content}', Description='{Description}', Amount={Amount}, TransferAmount={TransferAmount}", 
                        webhookId, simpleRequest.Content ?? "NULL", simpleRequest.Description ?? "NULL", simpleRequest.Amount, simpleRequest.TransferAmount?.ToString() ?? "NULL");
                    
                    // Ưu tiên dùng Content, nếu không có thì dùng Description (SePay format)
                    if (!string.IsNullOrEmpty(simpleRequest.Content))
                    {
                        content = simpleRequest.Content;
                        _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Using Content field: '{Content}'", webhookId, content);
                    }
                    else if (!string.IsNullOrEmpty(simpleRequest.Description))
                    {
                        _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Using Description field (SePay format): '{Description}'", webhookId, simpleRequest.Description);
                        content = simpleRequest.Description;
                    }
                    
                    // Ưu tiên dùng Amount, nếu không có thì dùng TransferAmount (SePay format)
                    if (simpleRequest.Amount > 0)
                    {
                        amount = simpleRequest.Amount;
                        _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Using Amount field: {Amount}", webhookId, amount);
                    }
                    else if (simpleRequest.TransferAmount.HasValue && simpleRequest.TransferAmount.Value > 0)
                    {
                        _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Using TransferAmount field (SePay format): {Amount}", webhookId, simpleRequest.TransferAmount.Value);
                        amount = simpleRequest.TransferAmount.Value;
                    }
                    
                    // Transaction ID
                    transactionId = simpleRequest.TransactionId ?? simpleRequest.ReferenceCode ?? simpleRequest.Id;
                    
                    _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Final extracted: Content='{Content}', Amount={Amount}, TransactionId='{TransactionId}'", 
                        webhookId, content ?? "NULL", amount, transactionId ?? "NULL");
                }
            }
            
            // If still no data, check if it's empty verification request
            if (string.IsNullOrEmpty(content) && amount == 0)
            {
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] PayOs verification request (empty data)", webhookId);
                return Ok(new
                {
                    status = "active",
                    endpoint = "/api/simplepayment/webhook",
                    message = "Webhook endpoint is ready",
                    timestamp = DateTime.UtcNow
                });
            }
            
            _logger.LogInformation("[WEBHOOK] 📥 Webhook received: {Content} - {Amount:N0} VND", content, amount);

            // Parse booking ID hoặc restaurant order ID từ content/description
            _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] ========== STARTING ID EXTRACTION ==========", webhookId);
            _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Current values: Content='{Content}', Amount={Amount}, OrderCode={OrderCode}", 
                webhookId, content ?? "NULL", amount, orderCode?.ToString() ?? "NULL");
            
            int? bookingId = null;
            int? restaurantOrderId = null;
            
            // Check if it's a restaurant order (format: ORDER{id} hoặc ORDER-{id})
            if (!string.IsNullOrEmpty(content))
            {
                var normalizedContent = content.ToUpper().Trim();
                
                // Pattern for restaurant order: "ORDER7" hoặc "ORDER-7"
                var orderPattern = @"ORDER[-_]?(\d+)";
                var orderMatch = Regex.Match(normalizedContent, orderPattern, RegexOptions.IgnoreCase);
                if (orderMatch.Success && orderMatch.Groups.Count > 1)
                {
                    if (int.TryParse(orderMatch.Groups[1].Value, out var orderId))
                    {
                        restaurantOrderId = orderId;
                        _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] ✅✅✅ SUCCESS: Extracted restaurant order ID from description: {OrderId}", webhookId, restaurantOrderId);
                    }
                }
                
                // If not restaurant order, try booking ID
                if (!restaurantOrderId.HasValue)
                {
                    _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Content is NOT empty, attempting to extract bookingId from: '{Content}'", webhookId, content);
                    bookingId = ExtractBookingId(content);
                    if (bookingId.HasValue)
                    {
                        _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] ✅✅✅ SUCCESS: Extracted bookingId from description: {BookingId}", webhookId, bookingId);
                    }
                    else
                    {
                        _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] ❌ FAILED: Could not extract bookingId from content: '{Content}'", webhookId, content);
                    }
                }
            }
            else
            {
                _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] Content is NULL or EMPTY, cannot extract ID from content", webhookId);
            }
            
            // Fallback: Nếu không extract được từ description, thử từ orderCode (chỉ khi orderCode nhỏ, có thể là ID cũ)
            if (!bookingId.HasValue && !restaurantOrderId.HasValue)
            {
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] ID not found from content, checking orderCode fallback...", webhookId);
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] OrderCode: {OrderCode}, Value > 0: {GreaterThanZero}, Value < 10000: {LessThan10000}", 
                    webhookId, orderCode?.ToString() ?? "NULL", orderCode.HasValue && orderCode.Value > 0, orderCode.HasValue && orderCode.Value < 10000);
                
                if (orderCode.HasValue && orderCode.Value > 0 && orderCode.Value < 10000)
                {
                    // Chỉ dùng orderCode nếu nó nhỏ hơn 10000 (có thể là bookingId cũ)
                    bookingId = (int)orderCode.Value;
                    _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] Using orderCode as bookingId (fallback): {BookingId}", webhookId, bookingId);
                }
                else
                {
                    _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] OrderCode fallback not applicable: OrderCode={OrderCode}", webhookId, orderCode?.ToString() ?? "NULL");
                }
            }
            
            // Process restaurant order payment if found
            if (restaurantOrderId.HasValue)
            {
                _logger.LogInformation("[WEBHOOK] 🔄 [WEBHOOK-{WebhookId}] Processing restaurant order payment for OrderId: {OrderId}", webhookId, restaurantOrderId.Value);
                
                var order = await _context.RestaurantOrders
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OrderId == restaurantOrderId.Value);
                
                if (order == null)
                {
                    _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] Restaurant order {OrderId} not found", webhookId, restaurantOrderId.Value);
                    return NotFound(new { message = $"Restaurant order {restaurantOrderId.Value} không tồn tại", webhookId });
                }
                
                _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] Restaurant order found: OrderNumber={OrderNumber}, Status={Status}, PaymentStatus={PaymentStatus}, Amount={Amount:N0} VND", 
                    webhookId, order.OrderNumber, order.Status, order.PaymentStatus, order.TotalAmount);
                
                // Check if already paid
                if (order.PaymentStatus == "Paid")
                {
                    _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] Restaurant order {OrderId} already paid, ignoring duplicate", webhookId, restaurantOrderId.Value);
                    return Ok(new { message = "Đã thanh toán rồi", orderId = restaurantOrderId.Value, webhookId });
                }
                
                // Verify amount
                if (amount > 0 && order.TotalAmount > 0)
                {
                    var diff = Math.Abs(amount - order.TotalAmount);
                    var maxDiff = order.TotalAmount * 0.1m;
                    
                    if (amount < order.TotalAmount && diff > maxDiff)
                    {
                        _logger.LogWarning("[WEBHOOK] ⚠️ Amount mismatch: Expected={Expected}, Received={Received}", 
                            order.TotalAmount, amount);
                        return BadRequest(new { message = "Số tiền không khớp" });
                    }
                    
                    _logger.LogInformation("[WEBHOOK] ✅ Amount verified: Expected={Expected}, Received={Received}, Diff={Diff}", 
                        order.TotalAmount, amount, diff);
                }
                
                // Update restaurant order payment status
                _logger.LogInformation("[WEBHOOK] 🔄 [WEBHOOK-{WebhookId}] Updating restaurant order {OrderId} to Paid status...", webhookId, restaurantOrderId.Value);
                order.PaymentMethod = "BankTransfer";
                order.PaymentStatus = "Paid";
                order.UpdatedAt = DateTime.UtcNow;
                
                // If status is Pending, update to Confirmed
                if (order.Status == "Pending")
                {
                    order.Status = "Confirmed";
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] Restaurant order {OrderId} ({OrderNumber}) updated to Paid successfully!", 
                    webhookId, restaurantOrderId.Value, order.OrderNumber);
                
                var restaurantDuration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogInformation("[WEBHOOK] ⏱️ [WEBHOOK-{WebhookId}] Processing time: {Duration}ms", webhookId, restaurantDuration);
                _logger.LogInformation("═══════════════════════════════════════════════════════════");
                
                return Ok(new
                {
                    success = true,
                    message = "Thanh toán thành công",
                    orderId = restaurantOrderId.Value,
                    orderNumber = order.OrderNumber,
                    type = "restaurant",
                    webhookId = webhookId,
                    processedAt = DateTime.UtcNow,
                    durationMs = restaurantDuration
                });
            }
            
            // Process booking payment if found
            if (!bookingId.HasValue)
            {
                _logger.LogError("[WEBHOOK] ❌ [WEBHOOK-{WebhookId}] ❌❌❌ CRITICAL: Cannot extract booking ID or restaurant order ID! Content: '{Content}', OrderCode: {OrderCode}", 
                    webhookId, content ?? "NULL", orderCode?.ToString() ?? "NULL");
                _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] ========== ID EXTRACTION FAILED ==========", webhookId);
                return BadRequest(new { message = "Không tìm thấy booking ID hoặc restaurant order ID trong nội dung", webhookId, content, orderCode });
            }
            
            _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] ✅✅✅ FINAL: Extracted booking ID: {BookingId}", webhookId, bookingId.Value);
            _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] ========== ID EXTRACTION COMPLETE ==========", webhookId);

            // Get booking
            _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Fetching booking {BookingId}...", webhookId, bookingId.Value);
            var booking = await _bookingService.GetBookingByIdAsync(bookingId.Value);
            if (booking == null)
            {
                _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] Booking {BookingId} not found", webhookId, bookingId.Value);
                return NotFound(new { message = $"Booking {bookingId.Value} không tồn tại", webhookId });
            }

            _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] Booking found: Code={BookingCode}, Status={Status}, Amount={Amount:N0} VND", 
                webhookId, booking.BookingCode, booking.Status, booking.EstimatedTotalAmount ?? 0);

            // Check if already paid
            if (booking.Status == "Paid")
            {
                _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] Booking {BookingId} already paid, ignoring duplicate", webhookId, bookingId.Value);
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
                    _logger.LogWarning("[WEBHOOK] ⚠️ Amount mismatch: Expected={Expected}, Received={Received}", 
                        estimatedAmount, amount);
                    return BadRequest(new { message = "Số tiền không khớp" });
                }
                
                _logger.LogInformation("[WEBHOOK] ✅ Amount verified: Expected={Expected}, Received={Received}, Diff={Diff}", 
                    estimatedAmount, amount, diff);
            }

            // Update booking status using ProcessOnlinePaymentAsync
            _logger.LogInformation("[WEBHOOK] 🔄 [WEBHOOK-{WebhookId}] ========== STARTING BOOKING STATUS UPDATE ==========", webhookId);
            _logger.LogInformation("[WEBHOOK] 🔄 [WEBHOOK-{WebhookId}] Updating booking {BookingId} to Paid status...", webhookId, bookingId.Value);
            _logger.LogInformation("[WEBHOOK] 🔄 [WEBHOOK-{WebhookId}] Current booking status BEFORE update: {Status}", webhookId, booking.Status);
            _logger.LogInformation("[WEBHOOK] 🔄 [WEBHOOK-{WebhookId}] Booking details: Code={BookingCode}, Amount={Amount:N0} VND", 
                webhookId, booking.BookingCode, booking.EstimatedTotalAmount ?? 0);
            
            var performedBy = $"Webhook-{transactionId ?? webhookId}";
            _logger.LogInformation("[WEBHOOK] 🔄 [WEBHOOK-{WebhookId}] Calling ProcessOnlinePaymentAsync with: BookingId={BookingId}, PerformedBy={PerformedBy}", 
                webhookId, bookingId.Value, performedBy);
            
            var updated = await _bookingService.ProcessOnlinePaymentAsync(bookingId.Value, performedBy);
            
            _logger.LogInformation("[WEBHOOK] 🔄 [WEBHOOK-{WebhookId}] ProcessOnlinePaymentAsync returned: {Updated}", webhookId, updated);
            
            if (!updated)
            {
                _logger.LogError("[WEBHOOK] ❌ [WEBHOOK-{WebhookId}] ❌❌❌ CRITICAL: Failed to update booking {BookingId}. ProcessOnlinePaymentAsync returned false", 
                    webhookId, bookingId.Value);
                _logger.LogInformation("[WEBHOOK] 🔄 [WEBHOOK-{WebhookId}] ========== BOOKING STATUS UPDATE FAILED ==========", webhookId);
                return StatusCode(500, new { message = "Không thể cập nhật booking", webhookId });
            }

            _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] ProcessOnlinePaymentAsync returned true, verifying booking status...", webhookId);

            // Verify booking was updated
            _logger.LogInformation("[WEBHOOK] 🔍 [WEBHOOK-{WebhookId}] Fetching updated booking to verify status change...", webhookId);
            var updatedBooking = await _bookingService.GetBookingByIdAsync(bookingId.Value);
            if (updatedBooking != null)
            {
                _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] Updated booking fetched successfully", webhookId);
                _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] Booking status AFTER update: {Status}", webhookId, updatedBooking.Status);
                _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] Status comparison: Before='{BeforeStatus}', After='{AfterStatus}', IsPaid={IsPaid}", 
                    webhookId, booking.Status, updatedBooking.Status, updatedBooking.Status == "Paid");
                
                if (updatedBooking.Status != "Paid")
                {
                    _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] ⚠️⚠️⚠️ WARNING: Booking status is NOT 'Paid' after update! Status: '{Status}'", 
                        webhookId, updatedBooking.Status);
                }
                else
                {
                    _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] ✅✅✅ SUCCESS: Booking status is 'Paid'!", webhookId);
                }
            }
            else
            {
                _logger.LogWarning("[WEBHOOK] ⚠️ [WEBHOOK-{WebhookId}] Could not fetch updated booking to verify status", webhookId);
            }
            
            _logger.LogInformation("[WEBHOOK] 🔄 [WEBHOOK-{WebhookId}] ========== BOOKING STATUS UPDATE COMPLETE ==========", webhookId);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("[WEBHOOK] ✅ [WEBHOOK-{WebhookId}] Booking {BookingId} ({BookingCode}) updated to Paid successfully!", 
                webhookId, bookingId.Value, booking.BookingCode);
            _logger.LogInformation("[WEBHOOK] ⏱️ [WEBHOOK-{WebhookId}] Processing time: {Duration}ms", webhookId, duration);
            _logger.LogInformation("═══════════════════════════════════════════════════════════");

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
            _logger.LogError(ex, "[WEBHOOK] ❌ [WEBHOOK-{WebhookId}] Error processing webhook after {Duration}ms", webhookId, duration);
            _logger.LogError("[WEBHOOK] ❌ [WEBHOOK-{WebhookId}] Error message: {Message}", webhookId, ex.Message);
            _logger.LogError("[WEBHOOK] ❌ [WEBHOOK-{WebhookId}] Stack trace: {StackTrace}", webhookId, ex.StackTrace);
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
            _logger.LogInformation("[BACKEND] 🔄 [CreateLink] Creating PayOs payment link for booking {BookingId}", request.BookingId);

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

            // Tạo orderCode unique để tránh conflict với PayOs
            // PayOs yêu cầu orderCode phải unique, nếu bookingId trùng sẽ báo lỗi "đã tồn tại"
            // Giải pháp: orderCode = bookingId * 10000 + timestamp (giây) để đảm bảo unique
            var timestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            var orderCode = request.BookingId * 10000L + (timestamp % 10000); // Đảm bảo unique
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

            if (paymentLink == null)
            {
                _logger.LogError("[BACKEND] ❌ [CreateLink] PayOs service returned null");
                return StatusCode(500, new { 
                    message = "Không thể tạo mã thanh toán. Vui lòng thử lại.",
                    error = "PayOs service returned null"
                });
            }

            if (paymentLink.Data == null)
            {
                // Nếu lỗi "Đơn thanh toán đã tồn tại", thử lấy payment link hiện có
                if (paymentLink.Desc?.Contains("đã tồn tại") == true || 
                    paymentLink.Desc?.Contains("already exists") == true ||
                    paymentLink.Code == "03")
                {
                    _logger.LogWarning("[BACKEND] ⚠️ [CreateLink] Payment link already exists for orderCode {OrderCode}. Trying to get existing link...", orderCode);
                    
                    var existingLink = await _payOsService.GetPaymentLinkByOrderCodeAsync(orderCode);
                    if (existingLink?.Data != null)
                    {
                        _logger.LogInformation("[BACKEND] ✅ [CreateLink] Found existing payment link: PaymentLinkId={PaymentLinkId}", 
                            existingLink.Data.PaymentLinkId);
                        
                        // Trả về payment link hiện có
                        return Ok(new
                        {
                            success = true,
                            paymentLinkId = existingLink.Data.PaymentLinkId,
                            orderCode = existingLink.Data.OrderCode,
                            qrCode = existingLink.Data.QrCode,
                            checkoutUrl = existingLink.Data.CheckoutUrl,
                            amount = existingLink.Data.Amount,
                            description = existingLink.Data.Description,
                            accountNumber = existingLink.Data.AccountNumber,
                            accountName = existingLink.Data.AccountName,
                            expiredAt = existingLink.Data.ExpiredAt
                        });
                    }
                }
                
                _logger.LogError("[BACKEND] ❌ [CreateLink] PayOs returned error. Code: {Code}, Desc: {Desc}", 
                    paymentLink.Code, paymentLink.Desc);
                return StatusCode(500, new { 
                    message = $"Không thể tạo mã thanh toán. {paymentLink.Desc ?? "Vui lòng thử lại."}",
                    code = paymentLink.Code,
                    desc = paymentLink.Desc,
                    error = "PayOs API returned error"
                });
            }

            _logger.LogInformation("[BACKEND] ✅ [CreateLink] Payment link created: PaymentLinkId={PaymentLinkId}", 
                paymentLink.Data.PaymentLinkId);
            
            // Log QR code details
            var hasQrCode = !string.IsNullOrEmpty(paymentLink.Data.QrCode);
            _logger.LogInformation("[BACKEND] 🔍 [CreateLink] QR Code in response: {HasQR}, Length: {Length}", 
                hasQrCode, paymentLink.Data.QrCode?.Length ?? 0);
            
            // Log account information để đảm bảo đúng tài khoản MB Bank
            _logger.LogInformation("[BACKEND] 🏦 [CreateLink] Account Number: {AccountNumber}, Account Name: {AccountName}", 
                paymentLink.Data.AccountNumber, paymentLink.Data.AccountName);
            
            // Validate account number - phải là 0901329227 (MB Bank)
            const string expectedAccountNumber = "0901329227";
            if (!string.IsNullOrEmpty(paymentLink.Data.AccountNumber) && 
                paymentLink.Data.AccountNumber != expectedAccountNumber)
            {
                _logger.LogWarning("[BACKEND] ⚠️ [CreateLink] Account Number mismatch! Expected: {Expected}, Got: {Actual}", 
                    expectedAccountNumber, paymentLink.Data.AccountNumber);
            }
            else if (paymentLink.Data.AccountNumber == expectedAccountNumber)
            {
                _logger.LogInformation("[BACKEND] ✅ [CreateLink] Account Number verified: {AccountNumber} (MB Bank)", 
                    paymentLink.Data.AccountNumber);
            }
            
            if (!hasQrCode)
            {
                _logger.LogWarning("[BACKEND] ⚠️ [CreateLink] PayOs did not return QR code. CheckoutUrl: {CheckoutUrl}", 
                    paymentLink.Data.CheckoutUrl);
            }

            return Ok(new
            {
                success = true,
                paymentLinkId = paymentLink.Data.PaymentLinkId,
                orderCode = paymentLink.Data.OrderCode,
                qrCode = paymentLink.Data.QrCode, // Base64 QR code image (may be null)
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
            _logger.LogError(ex, "[BACKEND] ❌ [CreateLink] Exception creating payment link: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "[BACKEND] ❌ [CreateLink] Inner exception: {Message}", ex.InnerException.Message);
            }
            _logger.LogError("[BACKEND] ❌ [CreateLink] Stack trace: {StackTrace}", ex.StackTrace);
            return StatusCode(500, new { 
                message = "Lỗi tạo mã thanh toán", 
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
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
        {
            _logger.LogWarning("[WEBHOOK] ExtractBookingId: Content is null or empty");
            return null;
        }

        // Normalize content: uppercase và trim
        var normalizedContent = content.ToUpper().Trim();
        _logger.LogInformation("[WEBHOOK] ExtractBookingId: Normalized content: '{NormalizedContent}'", normalizedContent);

        // Pattern 1: "BOOKING-39" hoặc "BOOKING_39" (có dấu gạch ngang/gạch dưới)
        var pattern1 = @"BOOKING[-_](\d+)";
        var match1 = Regex.Match(normalizedContent, pattern1, RegexOptions.IgnoreCase);
        if (match1.Success && match1.Groups.Count > 1)
        {
            if (int.TryParse(match1.Groups[1].Value, out var id))
            {
                _logger.LogInformation("[WEBHOOK] ExtractBookingId: ✅ Matched pattern1 'BOOKING-{Id}': {BookingId}", id, id);
                return id;
            }
        }

        // Pattern 2: "BOOKING7" hoặc "BOOKING39" (KHÔNG có dấu gạch ngang) - QUAN TRỌNG!
        // Pattern này sẽ match "CSHAX0QC6D9 BOOKING4" -> extract "4"
        var pattern2 = @"BOOKING(\d+)";
        var match2 = Regex.Match(normalizedContent, pattern2, RegexOptions.IgnoreCase);
        if (match2.Success && match2.Groups.Count > 1)
        {
            if (int.TryParse(match2.Groups[1].Value, out var id))
            {
                _logger.LogInformation("[WEBHOOK] ExtractBookingId: ✅ Matched pattern2 'BOOKING{Id}': {BookingId}", id, id);
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

    /// <summary>
    /// Tạo PayOs payment link cho restaurant order
    /// </summary>
    [HttpPost("create-link-restaurant")]
    [Authorize]
    public async Task<IActionResult> CreateRestaurantPaymentLink([FromBody] CreateRestaurantPaymentLinkRequest request)
    {
        try
        {
            _logger.LogInformation("[BACKEND] 🔄 [CreateRestaurantLink] Creating PayOs payment link for restaurant order {OrderId}", request.OrderId);

            // Get restaurant order
            var order = await _context.RestaurantOrders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);
            
            if (order == null)
            {
                return NotFound(new { message = $"Restaurant order {request.OrderId} không tồn tại" });
            }

            // Check authorization - customer chỉ có thể thanh toán đơn của mình
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            
            if (userRole == "Customer")
            {
                if (order.CustomerId == null)
                {
                    return BadRequest(new { message = "Đơn hàng này là đơn tại quầy, vui lòng thanh toán trực tiếp tại nhà hàng" });
                }
                
                if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId) || order.CustomerId != customerId)
                {
                    return StatusCode(403, new { message = "Bạn chỉ có thể thanh toán đơn hàng của chính bạn" });
                }
            }

            // Check if already paid
            if (order.PaymentStatus == "Paid")
            {
                return BadRequest(new { message = "Đơn hàng này đã được thanh toán" });
            }

            // Get amount
            var amount = order.TotalAmount;
            if (amount <= 0)
            {
                return BadRequest(new { message = "Số tiền thanh toán không hợp lệ" });
            }

            // Tạo orderCode unique - dùng format khác với booking để tránh conflict
            // Restaurant order: orderCode = 20000000 + orderId * 10000 + timestamp
            var timestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            var orderCode = 20000000L + request.OrderId * 10000L + (timestamp % 10000);
            var description = $"ORDER{request.OrderId}"; // PayOs description
            
            // Get base URL from request
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var returnUrl = $"{baseUrl}/customer/order-details.html?orderId={request.OrderId}&payment=success";
            var cancelUrl = $"{baseUrl}/customer/order-details.html?orderId={request.OrderId}&payment=cancelled";

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

            if (paymentLink == null)
            {
                _logger.LogError("[BACKEND] ❌ [CreateRestaurantLink] PayOs service returned null");
                return StatusCode(500, new { 
                    message = "Không thể tạo mã thanh toán. Vui lòng thử lại.",
                    error = "PayOs service returned null"
                });
            }

            if (paymentLink.Data == null)
            {
                // Nếu lỗi "Đơn thanh toán đã tồn tại", thử lấy payment link hiện có
                if (paymentLink.Desc?.Contains("đã tồn tại") == true || 
                    paymentLink.Desc?.Contains("already exists") == true ||
                    paymentLink.Code == "03")
                {
                    _logger.LogWarning("[BACKEND] ⚠️ [CreateRestaurantLink] Payment link already exists for orderCode {OrderCode}. Trying to get existing link...", orderCode);
                    
                    var existingLink = await _payOsService.GetPaymentLinkByOrderCodeAsync(orderCode);
                    if (existingLink?.Data != null)
                    {
                        _logger.LogInformation("[BACKEND] ✅ [CreateRestaurantLink] Found existing payment link: PaymentLinkId={PaymentLinkId}", 
                            existingLink.Data.PaymentLinkId);
                        
                        return Ok(new
                        {
                            success = true,
                            paymentLinkId = existingLink.Data.PaymentLinkId,
                            orderCode = existingLink.Data.OrderCode,
                            qrCode = existingLink.Data.QrCode,
                            checkoutUrl = existingLink.Data.CheckoutUrl,
                            amount = existingLink.Data.Amount,
                            description = existingLink.Data.Description,
                            accountNumber = existingLink.Data.AccountNumber,
                            accountName = existingLink.Data.AccountName,
                            expiredAt = existingLink.Data.ExpiredAt
                        });
                    }
                }
                
                _logger.LogError("[BACKEND] ❌ [CreateRestaurantLink] PayOs returned error. Code: {Code}, Desc: {Desc}", 
                    paymentLink.Code, paymentLink.Desc);
                return StatusCode(500, new { 
                    message = $"Không thể tạo mã thanh toán. {paymentLink.Desc ?? "Vui lòng thử lại."}",
                    code = paymentLink.Code,
                    desc = paymentLink.Desc,
                    error = "PayOs API returned error"
                });
            }

            _logger.LogInformation("[BACKEND] ✅ [CreateRestaurantLink] Payment link created: PaymentLinkId={PaymentLinkId}", 
                paymentLink.Data.PaymentLinkId);
            
            // Log QR code details
            var hasQrCode = !string.IsNullOrEmpty(paymentLink.Data.QrCode);
            _logger.LogInformation("[BACKEND] 🔍 [CreateRestaurantLink] QR Code in response: {HasQR}, Length: {Length}", 
                hasQrCode, paymentLink.Data.QrCode?.Length ?? 0);
            
            // Log account information
            _logger.LogInformation("[BACKEND] 🏦 [CreateRestaurantLink] Account Number: {AccountNumber}, Account Name: {AccountName}", 
                paymentLink.Data.AccountNumber, paymentLink.Data.AccountName);
            
            // Validate account number - phải là 0901329227 (MB Bank)
            const string expectedAccountNumber = "0901329227";
            if (!string.IsNullOrEmpty(paymentLink.Data.AccountNumber) && 
                paymentLink.Data.AccountNumber != expectedAccountNumber)
            {
                _logger.LogWarning("[BACKEND] ⚠️ [CreateRestaurantLink] Account Number mismatch! Expected: {Expected}, Got: {Actual}", 
                    expectedAccountNumber, paymentLink.Data.AccountNumber);
            }
            else if (paymentLink.Data.AccountNumber == expectedAccountNumber)
            {
                _logger.LogInformation("[BACKEND] ✅ [CreateRestaurantLink] Account Number verified: {AccountNumber} (MB Bank)", 
                    paymentLink.Data.AccountNumber);
            }
            
            if (!hasQrCode)
            {
                _logger.LogWarning("[BACKEND] ⚠️ [CreateRestaurantLink] PayOs did not return QR code. CheckoutUrl: {CheckoutUrl}", 
                    paymentLink.Data.CheckoutUrl);
            }

            return Ok(new
            {
                success = true,
                paymentLinkId = paymentLink.Data.PaymentLinkId,
                orderCode = paymentLink.Data.OrderCode,
                qrCode = paymentLink.Data.QrCode, // Base64 QR code image (may be null)
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
            _logger.LogError(ex, "[BACKEND] ❌ [CreateRestaurantLink] Exception creating payment link: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "[BACKEND] ❌ [CreateRestaurantLink] Inner exception: {Message}", ex.InnerException.Message);
            }
            _logger.LogError("[BACKEND] ❌ [CreateRestaurantLink] Stack trace: {StackTrace}", ex.StackTrace);
            return StatusCode(500, new { 
                message = "Lỗi tạo mã thanh toán", 
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// Endpoint để manually update booking status thành Paid (dùng khi webhook không hoạt động)
    /// </summary>
    [HttpPost("manual-update-paid/{bookingId}")]
    [Authorize(Roles = "Admin,FrontDesk,Manager")]
    public async Task<IActionResult> ManualUpdatePaid(int bookingId)
    {
        try
        {
            _logger.LogInformation("🔄 [ManualUpdate] Manually updating booking {BookingId} to Paid", bookingId);
            
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound(new { message = $"Booking {bookingId} không tồn tại" });
            }

            if (booking.Status == "Paid")
            {
                return Ok(new { message = "Booking đã được thanh toán rồi", bookingId, bookingCode = booking.BookingCode });
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Manual";
            var updated = await _bookingService.ProcessOnlinePaymentAsync(bookingId, userEmail);
            
            if (!updated)
            {
                return StatusCode(500, new { message = "Không thể cập nhật booking" });
            }

            _logger.LogInformation("✅ [ManualUpdate] Booking {BookingId} ({BookingCode}) updated to Paid", 
                bookingId, booking.BookingCode);

            return Ok(new 
            { 
                success = true,
                message = "Cập nhật thành công",
                bookingId,
                bookingCode = booking.BookingCode,
                status = "Paid"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ManualUpdate] Error updating booking {BookingId}", bookingId);
            return StatusCode(500, new { message = "Lỗi khi cập nhật booking", error = ex.Message });
        }
    }
}

/// <summary>
/// Request model cho webhook đơn giản (Simple format)
/// Hỗ trợ cả Simple format và SePay format
/// </summary>
public class SimpleWebhookRequest
{
    public string Content { get; set; } = string.Empty; // Nội dung chuyển khoản: "BOOKING-39"
    public decimal Amount { get; set; } // Số tiền
    public string? TransactionId { get; set; } // Mã giao dịch (optional)
    
    // SePay format fields
    [JsonPropertyName("description")]
    public string? Description { get; set; } // Mô tả (SePay format): "BOOKING4"
    [JsonPropertyName("id")]
    public string? Id { get; set; } // ID giao dịch (SePay format)
    [JsonPropertyName("referenceCode")]
    public string? ReferenceCode { get; set; } // Mã tham chiếu (SePay format)
    [JsonPropertyName("transferType")]
    public string? TransferType { get; set; } // Loại giao dịch: "IN", "OUT" (SePay format)
    [JsonPropertyName("transferAmount")]
    public decimal? TransferAmount { get; set; } // Số tiền giao dịch (SePay format)
    public string? AccountNumber { get; set; } // Số tài khoản
    public string? BankName { get; set; } // Tên ngân hàng
    public string? TransactionDate { get; set; } // Ngày giao dịch
}

/// <summary>
/// Request model cho PayOs webhook (PayOs format)
/// Format từ PayOs API documentation
/// </summary>
public class PayOsWebhookRequest
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty; // "00" = success
    
    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty; // "success"
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("data")]
    public PayOsWebhookData? Data { get; set; }
    
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>
/// Data trong PayOs webhook
/// Format theo PayOs API documentation: https://payos.vn/docs/api/
/// </summary>
public class PayOsWebhookData
{
    [JsonPropertyName("orderCode")]
    public long? OrderCode { get; set; } // Order code (PayOs gửi long, ví dụ: 123)
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; } // Số tiền (ví dụ: 3000)
    
    [JsonPropertyName("description")]
    public string? Description { get; set; } // Mô tả (có thể chứa booking ID: "BOOKING7" hoặc "VQRIO123")
    
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; } // Số tài khoản (ví dụ: "12345678")
    
    [JsonPropertyName("reference")]
    public string? Reference { get; set; } // Mã tham chiếu giao dịch (ví dụ: "TF230204212323")
    
    [JsonPropertyName("transactionDateTime")]
    public string? TransactionDateTime { get; set; } // Thời gian giao dịch (ví dụ: "2023-02-04 18:25:00")
    
    [JsonPropertyName("currency")]
    public string? Currency { get; set; } // Loại tiền tệ (ví dụ: "VND")
    
    [JsonPropertyName("paymentLinkId")]
    public string? PaymentLinkId { get; set; } // ID của payment link (ví dụ: "124c33293c43417ab7879e14c8d9eb18")
    
    // Các trường nested trong data (theo PayOs API documentation)
    [JsonPropertyName("code")]
    public string? Code { get; set; } // Code trong data (ví dụ: "00")
    
    [JsonPropertyName("desc")]
    public string? Desc { get; set; } // Mô tả trong data (ví dụ: "Thành công")
    
    // Thông tin tài khoản đối tác (counter account)
    [JsonPropertyName("counterAccountBankId")]
    public string? CounterAccountBankId { get; set; }
    
    [JsonPropertyName("counterAccountBankName")]
    public string? CounterAccountBankName { get; set; }
    
    [JsonPropertyName("counterAccountName")]
    public string? CounterAccountName { get; set; }
    
    [JsonPropertyName("counterAccountNumber")]
    public string? CounterAccountNumber { get; set; }
    
    // Thông tin tài khoản ảo (virtual account)
    [JsonPropertyName("virtualAccountName")]
    public string? VirtualAccountName { get; set; }
    
    [JsonPropertyName("virtualAccountNumber")]
    public string? VirtualAccountNumber { get; set; }
}

/// <summary>
/// Request để tạo PayOs payment link
/// </summary>
public class CreatePaymentLinkRequest
{
    public int BookingId { get; set; }
}

/// <summary>
/// Request để tạo PayOs payment link cho restaurant order
/// </summary>
public class CreateRestaurantPaymentLinkRequest
{
    public int OrderId { get; set; }
}

