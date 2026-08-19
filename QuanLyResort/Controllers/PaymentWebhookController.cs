using QuanLyResort.Repositories;
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
/// Controller don gi?n cho thanh toán - t?o PayOs payment link và x? lý webhook
/// </summary>
[ApiController]
    [Route("api/simplepayment")]
    public class PaymentWebhookController : ControllerBase
    {
    private readonly IBookingService _bookingService;
    private readonly PayOsService _payOsService;
    private readonly SePayService? _sePayService;
    private readonly VietQRService? _vietQRService;
    private readonly ILogger<PaymentWebhookController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private ResortDbContext _context => _unitOfWork.Context;

    public PaymentWebhookController(
        IBookingService bookingService,
        PayOsService payOsService,
        SePayService? sePayService,
        VietQRService? vietQRService,
        ILogger<PaymentWebhookController> logger,
        IUnitOfWork unitOfWork)
    {
        _bookingService = bookingService;
        _payOsService = payOsService;
        _sePayService = sePayService;
        _vietQRService = vietQRService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    /// <summary>
    /// Webhook don gi?n - nh?n t? PayOs/VietQR
    /// H? tr? 2 format:
    /// 1. PayOs format: { "code": "00", "desc": "success", "success": true, "data": { "orderCode": 123, "amount": 3000, "description": "BOOKING7", ... }, "signature": "..." }
    /// 2. Simple format: { "content": "BOOKING7", "amount": 5000, "transactionId": "..." }
    /// </summary>
    [HttpPost("webhook")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
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
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] PayOs verification request (empty body)", webhookId);
                // SePay yêu c?u response có success: true và HTTP Status Code 201 (ho?c 200)
                return StatusCode(201, new
                {
                    success = true,
                    status = "active",
                    endpoint = "/api/simplepayment/webhook",
                    message = "Webhook endpoint is ready",
                    timestamp = DateTime.UtcNow
                });
            }
            
            _logger.LogInformation("-----------------------------------------------------------");
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Webhook received at {Time}", webhookId, startTime);
            _logger.LogInformation("[WEBHOOK]    Raw request JSON: {RawRequest}", rawRequestJson);
            _logger.LogInformation("[WEBHOOK]    IP Address: {RemoteIp}", HttpContext.Connection.RemoteIpAddress?.ToString());
            _logger.LogInformation("[WEBHOOK]    User-Agent: {UserAgent}", Request.Headers["User-Agent"].ToString());
            
            // TODO: Verify SePay webhook signature n?u có SECRET_KEY
            // SePay có th? g?i signature trong header ho?c body
            // C?n implement signature verification khi có SECRET_KEY
            
            // Parse request - h? tr? c? PayOs format và Simple format
            string? content = null;
            decimal amount = 0;
            string? transactionId = null;
            long? orderCode = null;
            
            // Try PayOs format first
            PayOsWebhookRequest? payOsRequest = null;
            try
            {
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Attempting to deserialize as PayOs format...", webhookId);
                // C?u hình JsonSerializerOptions d? case-insensitive và cho phép trailing commas
                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Quan tr?ng: cho phép match lowercase v?i PascalCase
                    AllowTrailingCommas = true,
                    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
                };
                payOsRequest = System.Text.Json.JsonSerializer.Deserialize<PayOsWebhookRequest>(rawRequestJson, jsonOptions);
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] PayOs deserialization result: Code={Code}, Desc={Desc}, Success={Success}, Data={HasData}", 
                    webhookId, payOsRequest?.Code ?? "NULL", payOsRequest?.Desc ?? "NULL", payOsRequest?.Success, payOsRequest?.Data != null);
                
                if (payOsRequest != null)
                {
                    _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] PayOs request details: Code='{Code}', Desc='{Desc}', Success={Success}, Data is null: {DataIsNull}", 
                        webhookId, payOsRequest.Code, payOsRequest.Desc, payOsRequest.Success, payOsRequest.Data == null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Failed to deserialize as PayOs format: {Error}", webhookId, ex.Message);
                _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Exception type: {ExceptionType}, Stack trace: {StackTrace}", 
                    webhookId, ex.GetType().Name, ex.StackTrace);
            }
            
            // PayOs g?i "code": "00" cho success, có th? có field "success": true
            // Check c? Code và Data d? d?m b?o dúng format PayOs
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Checking PayOs format conditions: payOsRequest is null: {IsNull}, Code is empty: {CodeEmpty}, Data is null: {DataNull}", 
                webhookId, payOsRequest == null, string.IsNullOrEmpty(payOsRequest?.Code ?? ""), payOsRequest?.Data == null);
            
            if (payOsRequest != null)
            {
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] payOsRequest is NOT null, checking details...", webhookId);
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Code value: '{Code}' (IsEmpty: {IsEmpty})", 
                    webhookId, payOsRequest.Code ?? "NULL", string.IsNullOrEmpty(payOsRequest.Code));
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Data is null: {DataIsNull}", webhookId, payOsRequest.Data == null);
                
                if (!string.IsNullOrEmpty(payOsRequest.Code) && payOsRequest.Data != null)
                {
                    // PayOs format
                    _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] ? Detected PayOs format - entering PayOs processing block", webhookId);
                    content = payOsRequest.Data.Description; // PayOs g?i booking ID trong description
                    amount = payOsRequest.Data.Amount;
                    transactionId = payOsRequest.Data.Reference;
                    orderCode = payOsRequest.Data.OrderCode;
                    
                    _logger.LogInformation("[WEBHOOK]    PayOs - Code: {Code}, Desc: {Desc}", payOsRequest.Code, payOsRequest.Desc);
                    _logger.LogInformation("[WEBHOOK]    PayOs - OrderCode: {OrderCode}, Amount: {Amount:N0} VND", orderCode, amount);
                    _logger.LogInformation("[WEBHOOK]    PayOs - Description: '{Description}'", content);
                    _logger.LogInformation("[WEBHOOK]    PayOs - Reference: {Reference}", transactionId);
                    _logger.LogInformation("[WEBHOOK]    PayOs - Extracted content: '{Content}', amount: {Amount}", content, amount);
                    
                    // Ch? x? lý n?u code = "00" (success)
                    _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Checking PayOs code: '{Code}' == '00'? {IsSuccess}", 
                        webhookId, payOsRequest.Code, payOsRequest.Code == "00");
                    
                    if (payOsRequest.Code != "00")
                    {
                        _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] PayOs webhook failed with code: {Code}, Desc: {Desc}", 
                        webhookId, payOsRequest.Code, payOsRequest.Desc);
                        return Ok(new { message = $"Payment failed: {payOsRequest.Desc}", code = payOsRequest.Code });
                    }
                    
                    _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] PayOs code is '00' (success), continuing processing...", webhookId);

                    // ----------------------------------------------------
                    // 1. VERIFY PAYOS SIGNATURE
                    // ----------------------------------------------------
                    var payOsConfig = configuration.GetSection("BankWebhook:PayOs");
                    var checksumKey = payOsConfig["ChecksumKey"] ?? payOsConfig["SecretKey"];
                    
                    if (!string.IsNullOrEmpty(checksumKey))
                    {
                        try
                        {
                            using var jsonDoc = System.Text.Json.JsonDocument.Parse(rawRequestJson);
                            if (jsonDoc.RootElement.TryGetProperty("signature", out var signatureElement) && 
                                jsonDoc.RootElement.TryGetProperty("data", out var dataElement))
                            {
                                var providedSignature = signatureElement.GetString();
                                
                                var sortedData = new SortedDictionary<string, string>();
                                foreach (var prop in dataElement.EnumerateObject())
                                {
                                    if (prop.Value.ValueKind != System.Text.Json.JsonValueKind.Null && 
                                        prop.Value.ValueKind != System.Text.Json.JsonValueKind.Object && 
                                        prop.Value.ValueKind != System.Text.Json.JsonValueKind.Array)
                                    {
                                        sortedData.Add(prop.Name, prop.Value.ToString());
                                    }
                                }
                                
                                var signDataStr = string.Join("&", sortedData.Select(kv => $"{kv.Key}={kv.Value}"));
                                
                                using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(checksumKey));
                                var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signDataStr));
                                var computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                                
                                if (!string.Equals(computedSignature, providedSignature, StringComparison.OrdinalIgnoreCase))
                                {
                                    _logger.LogWarning("[WEBHOOK] ? PayOS Signature mismatch. Expected: {Expected}, Received: {Received}", computedSignature, providedSignature);
                                    return StatusCode(403, new { message = "Invalid PayOS signature" });
                                }
                                _logger.LogInformation("[WEBHOOK] ? PayOS Signature verified successfully.");
                            }
                            else
                            {
                                _logger.LogWarning("[WEBHOOK] ? PayOS Webhook missing signature or data field");
                                return StatusCode(403, new { message = "Missing PayOS signature" });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("[WEBHOOK] ?? Failed to verify PayOS signature: {Error}", ex.Message);
                            return StatusCode(500, new { message = "Error verifying signature" });
                        }
                    }
                    else 
                    {
                        _logger.LogWarning("[WEBHOOK] ?? PayOS ChecksumKey is not configured. Skipping signature verification.");
                    }
                }
                else
                {
                    _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] PayOs format check failed: Code empty={CodeEmpty}, Data null={DataNull}", 
                        webhookId, string.IsNullOrEmpty(payOsRequest.Code), payOsRequest.Data == null);
                }
            }
            else
            {
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] payOsRequest is NULL, will try Simple format", webhookId);
            }
            
            // Check if we successfully extracted PayOs data
            if (payOsRequest != null && !string.IsNullOrEmpty(payOsRequest.Code) && payOsRequest.Data != null && payOsRequest.Code == "00")
            {
                // PayOs format successfully processed - continue with extracted data
                _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] PayOs format successfully processed, extracted data: Content='{Content}', Amount={Amount}", 
                    webhookId, content, amount);
            }
            else
            {
                // Try Simple format
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] PayOs format not detected, trying Simple format...", webhookId);
                SimpleWebhookRequest? simpleRequest = null;
                try
                {
                    var jsonOptions = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true
                    };
                    simpleRequest = System.Text.Json.JsonSerializer.Deserialize<SimpleWebhookRequest>(rawRequestJson, jsonOptions);
                    _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Simple deserialization result: Content={Content}, Amount={Amount}, TransferAmount={TransferAmount}", 
                        webhookId, simpleRequest?.Content ?? "NULL", simpleRequest?.Amount ?? 0, simpleRequest?.TransferAmount?.ToString() ?? "NULL");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Failed to deserialize as Simple format: {Error}", webhookId, ex.Message);
                }
                
                if (simpleRequest != null)
                {
                    _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Detected Simple/SePay format", webhookId);

                    // ----------------------------------------------------
                    // 2. VERIFY SEPAY WEBHOOK - SePay không g?i Auth header
                    // ----------------------------------------------------
                    // NOTE: SePay ApiToken ch? dùng d? G?I SePay API (outbound),
                    // không ph?i d? xác th?c webhook d?n (inbound).
                    // SePay webhook không g?i Authorization header theo m?c d?nh.
                    // Xác th?c du?c th?c hi?n qua n?i dung (content) và IP whitelist n?u c?n.
                    _logger.LogInformation("[WEBHOOK] ? SePay webhook received - skipping API key check (SePay does not send auth header)");

                    _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] SePay request fields: Id={Id}, Gateway={Gateway}, Content='{Content}', Description='{Description}', TransferAmount={TransferAmount}, TransferType={TransferType}, ReferenceCode={ReferenceCode}", 
                        webhookId, simpleRequest.Id?.ToString() ?? "NULL", simpleRequest.Gateway ?? "NULL", 
                        simpleRequest.Content ?? "NULL", simpleRequest.Description ?? "NULL", 
                        simpleRequest.TransferAmount?.ToString() ?? "NULL", simpleRequest.TransferType ?? "NULL",
                        simpleRequest.ReferenceCode ?? "NULL");
                    
                    // SePay format: Uu tiên dùng Content (n?i dung chuy?n kho?n), n?u không có thì dùng Description
                    // Content thu?ng ch?a "BOOKING4", "ORDER7", etc.
                    if (!string.IsNullOrEmpty(simpleRequest.Content))
                    {
                        content = simpleRequest.Content.Trim();
                        _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Using Content field (SePay): '{Content}'", webhookId, content);
                    }
                    else if (!string.IsNullOrEmpty(simpleRequest.Description))
                    {
                        content = simpleRequest.Description.Trim();
                        _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Using Description field (SePay fallback): '{Description}'", webhookId, content);
                    }
                    
                    // SePay format: Uu tiên dùng TransferAmount, n?u không có thì dùng Amount (legacy)
                    if (simpleRequest.TransferAmount.HasValue && simpleRequest.TransferAmount.Value > 0)
                    {
                        amount = simpleRequest.TransferAmount.Value;
                        _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Using TransferAmount field (SePay): {Amount:N0} VND", webhookId, amount);
                    }
                    else if (simpleRequest.Amount > 0)
                    {
                        amount = simpleRequest.Amount;
                        _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Using Amount field (legacy fallback): {Amount:N0} VND", webhookId, amount);
                    }
                    
                    // Transaction ID: Uu tiên dùng Id (int), sau dó ReferenceCode, sau dó TransactionId (legacy)
                    if (simpleRequest.Id.HasValue)
                    {
                        transactionId = simpleRequest.Id.Value.ToString();
                        _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Using Id field (SePay): {TransactionId}", webhookId, transactionId);
                    }
                    else if (!string.IsNullOrEmpty(simpleRequest.ReferenceCode))
                    {
                        transactionId = simpleRequest.ReferenceCode;
                        _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Using ReferenceCode field (SePay): {TransactionId}", webhookId, transactionId);
                    }
                    else if (!string.IsNullOrEmpty(simpleRequest.TransactionId))
                    {
                        transactionId = simpleRequest.TransactionId;
                        _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Using TransactionId field (legacy fallback): {TransactionId}", webhookId, transactionId);
                    }
                    
                    // Log thông tin b? sung t? SePay
                    if (!string.IsNullOrEmpty(simpleRequest.Gateway))
                    {
                        _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Bank Gateway: {Gateway}", webhookId, simpleRequest.Gateway);
                    }
                    if (!string.IsNullOrEmpty(simpleRequest.AccountNumber))
                    {
                        _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Account Number: {AccountNumber}", webhookId, simpleRequest.AccountNumber);
                    }
                    if (!string.IsNullOrEmpty(simpleRequest.TransferType))
                    {
                        _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Transfer Type: {TransferType}", webhookId, simpleRequest.TransferType);
                    }
                    
                    _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Final extracted: Content='{Content}', Amount={Amount:N0} VND, TransactionId='{TransactionId}'", 
                        webhookId, content ?? "NULL", amount, transactionId ?? "NULL");
                }
            }
            
            // If still no data, check if it's empty verification request
            if (string.IsNullOrEmpty(content) && amount == 0)
            {
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] PayOs verification request (empty data)", webhookId);
                // SePay yêu c?u response có success: true và HTTP Status Code 201 (ho?c 200)
                return StatusCode(201, new
                {
                    success = true,
                    status = "active",
                    endpoint = "/api/simplepayment/webhook",
                    message = "Webhook endpoint is ready",
                    timestamp = DateTime.UtcNow
                });
            }
            
            _logger.LogInformation("[WEBHOOK] ?? Webhook received: {Content} - {Amount:N0} VND", content, amount);

            // Parse booking ID ho?c restaurant order ID t? content/description
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] ========== STARTING ID EXTRACTION ==========", webhookId);
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Current values: Content='{Content}', Amount={Amount}, OrderCode={OrderCode}", 
                webhookId, content ?? "NULL", amount, orderCode?.ToString() ?? "NULL");
            
            int? bookingId = null;
            int? restaurantOrderId = null;
            
            // Check if it's a restaurant order (format: ORDER{id} ho?c ORDER-{id})
            if (!string.IsNullOrEmpty(content))
            {
                var normalizedContent = content.ToUpper().Trim();
                
                // Pattern for restaurant order: "ORDER7" ho?c "ORDER-7"
                var orderPattern = @"ORDER[-_]?(\d+)";
                var orderMatch = Regex.Match(normalizedContent, orderPattern, RegexOptions.IgnoreCase);
                if (orderMatch.Success && orderMatch.Groups.Count > 1)
                {
                    if (int.TryParse(orderMatch.Groups[1].Value, out var orderId))
                    {
                        restaurantOrderId = orderId;
                        _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] ??? SUCCESS: Extracted restaurant order ID from description: {OrderId}", webhookId, restaurantOrderId);
                    }
                }
                
                // If not restaurant order, try booking ID
                if (!restaurantOrderId.HasValue)
                {
                    _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Content is NOT empty, attempting to extract bookingId from: '{Content}'", webhookId, content);
                    bookingId = ExtractBookingId(content);
                    if (bookingId.HasValue)
                    {
                        _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] ??? SUCCESS: Extracted bookingId from description: {BookingId}", webhookId, bookingId);
                    }
                    else
                    {
                        _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] ? FAILED: Could not extract bookingId from content: '{Content}'", webhookId, content);
                    }
                }
            }
            else
            {
                _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Content is NULL or EMPTY, cannot extract ID from content", webhookId);
            }
            
            // Fallback: N?u không extract du?c t? description, th? t? orderCode (ch? khi orderCode nh?, có th? là ID cu)
            if (!bookingId.HasValue && !restaurantOrderId.HasValue)
            {
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] ID not found from content, checking orderCode fallback...", webhookId);
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] OrderCode: {OrderCode}, Value > 0: {GreaterThanZero}, Value < 10000: {LessThan10000}", 
                    webhookId, orderCode?.ToString() ?? "NULL", orderCode.HasValue && orderCode.Value > 0, orderCode.HasValue && orderCode.Value < 10000);
                
                if (orderCode.HasValue && orderCode.Value > 0 && orderCode.Value < 10000)
                {
                    // Ch? dùng orderCode n?u nó nh? hon 10000 (có th? là bookingId cu)
                    bookingId = (int)orderCode.Value;
                    _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Using orderCode as bookingId (fallback): {BookingId}", webhookId, bookingId);
                }
                else
                {
                    _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] OrderCode fallback not applicable: OrderCode={OrderCode}", webhookId, orderCode?.ToString() ?? "NULL");
                }
            }
            
            // Process restaurant order payment if found
            if (restaurantOrderId.HasValue)
            {
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Processing restaurant order payment for OrderId: {OrderId}", webhookId, restaurantOrderId.Value);
                
                var order = await _context.RestaurantOrders
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OrderId == restaurantOrderId.Value);
                
                if (order == null)
                {
                    _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Restaurant order {OrderId} not found", webhookId, restaurantOrderId.Value);
                    return NotFound(new { message = $"Restaurant order {restaurantOrderId.Value} không t?n t?i", webhookId });
                }
                
                _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Restaurant order found: OrderNumber={OrderNumber}, Status={Status}, PaymentStatus={PaymentStatus}, Amount={Amount:N0} VND", 
                    webhookId, order.OrderNumber, order.Status, order.PaymentStatus, order.TotalAmount);
                
                // Check if already paid
                if (order.PaymentStatus == "Paid")
                {
                    _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Restaurant order {OrderId} already paid, ignoring duplicate", webhookId, restaurantOrderId.Value);
                    // SePay yêu c?u response có success: true và HTTP Status Code 201 (ho?c 200)
                    return StatusCode(201, new { success = true, message = "Ðã thanh toán r?i", orderId = restaurantOrderId.Value, webhookId });
                }
                
                // Verify amount
                if (amount > 0 && order.TotalAmount > 0)
                {
                    var diff = Math.Abs(amount - order.TotalAmount);
                    var maxDiff = order.TotalAmount * 0.1m;
                    
                    if (amount < order.TotalAmount && diff > maxDiff)
                    {
                        _logger.LogWarning("[WEBHOOK] ?? Amount mismatch: Expected={Expected}, Received={Received}", 
                            order.TotalAmount, amount);
                        return BadRequest(new { message = "S? ti?n không kh?p" });
                    }
                    
                    _logger.LogInformation("[WEBHOOK] ? Amount verified: Expected={Expected}, Received={Received}, Diff={Diff}", 
                        order.TotalAmount, amount, diff);
                }
                
                // Update restaurant order payment status
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Updating restaurant order {OrderId} to Paid status...", webhookId, restaurantOrderId.Value);
                order.PaymentMethod = "BankTransfer";
                order.PaymentStatus = "Paid";
                order.UpdatedAt = DateTime.UtcNow;
                
                // If status is Pending, update to Confirmed
                if (order.Status == "Pending")
                {
                    order.Status = "Confirmed";
                }
                
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Restaurant order {OrderId} ({OrderNumber}) updated to Paid successfully!", 
                    webhookId, restaurantOrderId.Value, order.OrderNumber);
                
                var restaurantDuration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Processing time: {Duration}ms", webhookId, restaurantDuration);
                _logger.LogInformation("-----------------------------------------------------------");
                
                // SePay yêu c?u response có success: true và HTTP Status Code 201 (ho?c 200)
                return StatusCode(201, new
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
                _logger.LogError("[WEBHOOK] ? [WEBHOOK-{WebhookId}] ??? CRITICAL: Cannot extract booking ID or restaurant order ID! Content: '{Content}', OrderCode: {OrderCode}", 
                    webhookId, content ?? "NULL", orderCode?.ToString() ?? "NULL");
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] ========== ID EXTRACTION FAILED ==========", webhookId);
                return BadRequest(new { message = "Không tìm th?y booking ID ho?c restaurant order ID trong n?i dung", webhookId, content, orderCode });
            }
            
            _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] ??? FINAL: Extracted booking ID: {BookingId}", webhookId, bookingId.Value);
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] ========== ID EXTRACTION COMPLETE ==========", webhookId);

            // Get booking
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Fetching booking {BookingId}...", webhookId, bookingId.Value);
            var booking = await _bookingService.GetBookingByIdAsync(bookingId.Value);
            if (booking == null)
            {
                _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Booking {BookingId} not found in database", webhookId, bookingId.Value);
                _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Webhook extract du?c booking ID = {BookingId} nhung booking này không t?n t?i trong database", webhookId, bookingId.Value);
                _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Có th?: 1) Booking dã b? xóa, 2) Booking ID trong n?i dung chuy?n kho?n sai, 3) Database không có booking này", webhookId);
                return NotFound(new { 
                    message = $"Booking {bookingId.Value} không t?n t?i trong database. Vui lòng ki?m tra booking ID trong n?i dung chuy?n kho?n.", 
                    webhookId,
                    extractedBookingId = bookingId.Value,
                    suggestion = "Ki?m tra: 1) Booking có t?n t?i không, 2) N?i dung chuy?n kho?n có dúng format BOOKING{id} không"
                });
            }

            _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Booking found: Code={BookingCode}, Status={Status}, Amount={Amount:N0} VND", 
                webhookId, booking.BookingCode, booking.Status, booking.EstimatedTotalAmount ?? 0);

            // Check if already paid
            if (booking.Status == "Paid")
            {
                _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Booking {BookingId} already paid, ignoring duplicate", webhookId, bookingId.Value);
                // SePay yêu c?u response có success: true và HTTP Status Code 201 (ho?c 200)
                return StatusCode(201, new { success = true, message = "Ðã thanh toán r?i", bookingId = bookingId.Value, webhookId });
            }

            // Verify amount (optional - có th? b? qua n?u mu?n don gi?n hon)
            var estimatedAmount = booking.EstimatedTotalAmount ?? 0;
            if (amount > 0 && estimatedAmount > 0)
            {
                 // Cho phép sai s? 10% ho?c ch?p nh?n n?u amount >= expected
                var diff = Math.Abs(amount - estimatedAmount);
                var maxDiff = estimatedAmount * 0.1m;
                
                // Ch?p nh?n n?u:
                // 1. Amount >= estimatedAmount (thanh toán d? ho?c nhi?u hon)
                // 2. Ho?c sai s? <= 10%
                if (amount < estimatedAmount && diff > maxDiff)
                {
                    _logger.LogWarning("[WEBHOOK] ?? Amount mismatch: Expected={Expected}, Received={Received}", 
                        estimatedAmount, amount);
                    return BadRequest(new { message = "S? ti?n không kh?p" });
                }
                
                _logger.LogInformation("[WEBHOOK] ? Amount verified: Expected={Expected}, Received={Received}, Diff={Diff}", 
                    estimatedAmount, amount, diff);
            }

            // Update booking status using ProcessOnlinePaymentAsync
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] ========== STARTING BOOKING STATUS UPDATE ==========", webhookId);
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Updating booking {BookingId} to Paid status...", webhookId, bookingId.Value);
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Current booking status BEFORE update: {Status}", webhookId, booking.Status);
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Booking details: Code={BookingCode}, Amount={Amount:N0} VND", 
                webhookId, booking.BookingCode, booking.EstimatedTotalAmount ?? 0);
            
            var performedBy = $"Webhook-{transactionId ?? webhookId}";
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Calling ProcessOnlinePaymentAsync with: BookingId={BookingId}, PerformedBy={PerformedBy}", 
                webhookId, bookingId.Value, performedBy);
            
            var updated = await _bookingService.ProcessOnlinePaymentAsync(bookingId.Value, performedBy);
            
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] ProcessOnlinePaymentAsync returned: {Updated}", webhookId, updated);
            
            if (!updated)
            {
                _logger.LogError("[WEBHOOK] ? [WEBHOOK-{WebhookId}] ??? CRITICAL: Failed to update booking {BookingId}. ProcessOnlinePaymentAsync returned false", 
                    webhookId, bookingId.Value);
                _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] ========== BOOKING STATUS UPDATE FAILED ==========", webhookId);
                return StatusCode(500, new { message = "Không th? c?p nh?t booking", webhookId });
            }

            _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] ProcessOnlinePaymentAsync returned true, verifying booking status...", webhookId);

            // Verify booking was updated
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Fetching updated booking to verify status change...", webhookId);
            var updatedBooking = await _bookingService.GetBookingByIdAsync(bookingId.Value);
            if (updatedBooking != null)
            {
                _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Updated booking fetched successfully", webhookId);
                _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Booking status AFTER update: {Status}", webhookId, updatedBooking.Status);
                _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Status comparison: Before='{BeforeStatus}', After='{AfterStatus}', IsPaid={IsPaid}", 
                    webhookId, booking.Status, updatedBooking.Status, updatedBooking.Status == "Paid");
                
                if (updatedBooking.Status != "Paid")
                {
                    _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] ?????? WARNING: Booking status is NOT 'Paid' after update! Status: '{Status}'", 
                        webhookId, updatedBooking.Status);
                }
                else
                {
                    _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] ??? SUCCESS: Booking status is 'Paid'!", webhookId);
                }
            }
            else
            {
                _logger.LogWarning("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Could not fetch updated booking to verify status", webhookId);
            }
            
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] ========== BOOKING STATUS UPDATE COMPLETE ==========", webhookId);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Booking {BookingId} ({BookingCode}) updated to Paid successfully!", 
                webhookId, bookingId.Value, booking.BookingCode);
            _logger.LogInformation("[WEBHOOK] ?? [WEBHOOK-{WebhookId}] Processing time: {Duration}ms", webhookId, duration);
            _logger.LogInformation("-----------------------------------------------------------");

            // SePay yêu c?u response có success: true và HTTP Status Code 201 (ho?c 200)
            // Dùng StatusCode(201) d? d?m b?o SePay nh?n du?c response thành công
            return StatusCode(201, new
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
            _logger.LogError(ex, "[WEBHOOK] ? [WEBHOOK-{WebhookId}] Error processing webhook after {Duration}ms", webhookId, duration);
            _logger.LogError("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Error message: {Message}", webhookId, ex.Message);
            _logger.LogError("[WEBHOOK] ? [WEBHOOK-{WebhookId}] Stack trace: {StackTrace}", webhookId, ex.StackTrace);
            _logger.LogInformation("-----------------------------------------------------------");
            return StatusCode(500, new { message = "L?i x? lý webhook", error = ex.Message, webhookId });
        }
    }
    /// <summary>
    /// Endpoint d? PayOs verify webhook URL (GET request)
    /// PayOs s? g?i GET request d? verify webhook URL tru?c khi ch?p nh?n
    /// </summary>
    [HttpGet("webhook")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public IActionResult VerifyWebhook()
    {
        _logger.LogInformation("?? [WEBHOOK-VERIFY] PayOs verification request received");
        return Ok(new
        {
            status = "active",
            endpoint = "/api/simplepayment/webhook",
            message = "Webhook endpoint is ready",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Endpoint d? ki?m tra tr?ng thái webhook system
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
    /// Extract booking ID t? content
    /// Format: "BOOKING-39", "BOOKING7", "BOOKING-BKG2025039", ho?c ch? s? "7"
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

        // Pattern 1: "BOOKING-39" ho?c "BOOKING_39" (có d?u g?ch ngang/g?ch du?i)
        var pattern1 = @"BOOKING[-_](\d+)";
        var match1 = Regex.Match(normalizedContent, pattern1, RegexOptions.IgnoreCase);
        if (match1.Success && match1.Groups.Count > 1)
        {
            if (int.TryParse(match1.Groups[1].Value, out var id))
            {
                _logger.LogInformation("[WEBHOOK] ExtractBookingId: ? Matched pattern1 'BOOKING-{Id}': {BookingId}", id, id);
                return id;
            }
        }

        // Pattern 2: "BOOKING7" ho?c "BOOKING39" (KHÔNG có d?u g?ch ngang) - QUAN TR?NG!
        // Pattern này s? match "CSHAX0QC6D9 BOOKING4" -> extract "4"
        var pattern2 = @"BOOKING(\d+)";
        var match2 = Regex.Match(normalizedContent, pattern2, RegexOptions.IgnoreCase);
        if (match2.Success && match2.Groups.Count > 1)
        {
            if (int.TryParse(match2.Groups[1].Value, out var id))
            {
                _logger.LogInformation("[WEBHOOK] ExtractBookingId: ? Matched pattern2 'BOOKING{Id}': {BookingId}", id, id);
                return id;
            }
        }

        // Pattern 3: "BOOKING-BKG2025039" -> extract "39" t? cu?i
        var pattern3 = @"BOOKING[-_]?BKG\d+(\d{1,3})";
        var match3 = Regex.Match(normalizedContent, pattern3, RegexOptions.IgnoreCase);
        if (match3.Success && match3.Groups.Count > 1)
        {
            if (int.TryParse(match3.Groups[1].Value, out var id))
                return id;
        }

        // Pattern 4: Ch? s? (n?u h?p lý: 1-9999)
        if (int.TryParse(normalizedContent, out var directId) && directId > 0 && directId < 10000)
            return directId;

        return null;
    }
}

