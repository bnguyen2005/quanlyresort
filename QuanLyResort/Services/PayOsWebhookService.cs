using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using QuanLyResort.Hubs;

namespace QuanLyResort.Services;

/// <summary>
/// Service xử lý webhook từ PayOs (MB Bank Payment Gateway)
/// PayOs là dịch vụ thanh toán của MB Bank
/// </summary>
public class PayOsWebhookService
{
    private readonly IBankWebhookService _bankWebhookService;
    private readonly IPaymentSessionService _paymentSessionService;
    private readonly IHubContext<PaymentHub> _hubContext;
    private readonly ILogger<PayOsWebhookService> _logger;
    private readonly IConfiguration _configuration;

    public PayOsWebhookService(
        IBankWebhookService bankWebhookService,
        IPaymentSessionService paymentSessionService,
        IHubContext<PaymentHub> hubContext,
        ILogger<PayOsWebhookService> logger,
        IConfiguration configuration)
    {
        _bankWebhookService = bankWebhookService;
        _paymentSessionService = paymentSessionService;
        _hubContext = hubContext;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Xử lý webhook từ PayOs
    /// </summary>
    public async Task<BankWebhookResult> ProcessPayOsWebhookAsync(PayOsWebhookDto dto)
    {
        var webhookId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            _logger.LogInformation("📥 [PAYOS-WEBHOOK-{WebhookId}] Processing PayOs webhook", webhookId);
            _logger.LogInformation("   TransactionId: {TransactionId}", dto.Data?.TransactionId ?? "N/A");
            _logger.LogInformation("   Amount: {Amount:N0} VND", dto.Data?.Amount ?? 0);
            _logger.LogInformation("   Code: {Code} ({Desc})", dto.Code, dto.Desc ?? "N/A");
            _logger.LogInformation("   Description: {Description}", dto.Data?.Description ?? "N/A");
            
            Console.WriteLine($"\n📥 [PAYOS-WEBHOOK-{webhookId}] PayOs: {dto.Data?.Description} - {dto.Data?.Amount:N0} VND (Code: {dto.Code})");

            // Verify signature
            _logger.LogInformation("🔍 [PAYOS-WEBHOOK-{WebhookId}] Verifying signature...", webhookId);
            var isValid = VerifySignature(dto, dto.Signature);
            if (!isValid)
            {
                _logger.LogWarning("⚠️ [PAYOS-WEBHOOK-{WebhookId}] Invalid PayOs webhook signature", webhookId);
                Console.WriteLine($"⚠️ [PAYOS-WEBHOOK-{webhookId}] Invalid signature");
                return new BankWebhookResult
                {
                    Success = false,
                    Message = "Invalid signature"
                };
            }

            // Kiểm tra code - 0 = thành công
            if (dto.Code != 0)
            {
                _logger.LogWarning("⚠️ [PAYOS-WEBHOOK-{WebhookId}] PayOs webhook failed with code: {Code}, Desc: {Desc}", 
                    webhookId, dto.Code, dto.Desc);
                Console.WriteLine($"❌ [PAYOS-WEBHOOK-{webhookId}] Payment failed: Code {dto.Code} - {dto.Desc}");
                return new BankWebhookResult
                {
                    Success = false,
                    Message = $"Payment failed: {dto.Desc}"
                };
            }

            // Map PayOs format to BankWebhookRequest
            var bankRequest = new BankWebhookRequest
            {
                BankName = "MB", // PayOs là của MB Bank
                TransactionId = dto.Data?.TransactionId ?? dto.Id ?? string.Empty,
                Amount = dto.Data?.Amount ?? 0,
                Content = dto.Data?.Description ?? string.Empty,
                AccountNumber = dto.Data?.AccountNumber ?? string.Empty,
                AccountName = dto.Data?.AccountName ?? "Resort Deluxe",
                TransactionDate = dto.Data?.TransactionDateTime ?? DateTime.UtcNow,
                Signature = dto.Signature,
                RawData = new Dictionary<string, object>
                {
                    { "payos_code", dto.Code },
                    { "payos_desc", dto.Desc ?? string.Empty },
                    { "payos_id", dto.Id ?? string.Empty },
                    { "payos_status", dto.Data?.Status ?? string.Empty }
                }
            };

            // Process through main webhook service
            _logger.LogInformation("🔄 [PAYOS-WEBHOOK-{WebhookId}] Processing through BankWebhookService...", webhookId);
            var result = await _bankWebhookService.ProcessWebhookAsync(bankRequest);

            // Nếu thành công, broadcast qua SignalR để frontend cập nhật real-time
            if (result.Success && result.BookingId.HasValue)
            {
                var broadcastTasks = new List<Task>();

                // Broadcast cho payment session nếu có
                if (!string.IsNullOrEmpty(result.PaymentSessionId))
                {
                    broadcastTasks.Add(_hubContext.Clients.Group($"payment_{result.PaymentSessionId}").SendAsync("PaymentStatusChanged", new
                    {
                        sessionId = result.PaymentSessionId,
                        bookingId = result.BookingId.Value,
                        status = "paid",
                        transactionId = bankRequest.TransactionId,
                        invoiceNumber = $"INV-{result.BookingId}",
                        paidAt = bankRequest.TransactionDate,
                        errorMessage = (string?)null
                    }));
                }

                // Broadcast cho booking group (fallback cho các client không có payment session)
                broadcastTasks.Add(_hubContext.Clients.Group($"booking_{result.BookingId.Value}").SendAsync("BookingStatusChanged", new
                {
                    bookingId = result.BookingId.Value,
                    status = "Paid",
                    transactionId = bankRequest.TransactionId,
                    paidAt = bankRequest.TransactionDate
                }));

                // Wait for all broadcasts
                await Task.WhenAll(broadcastTasks);

                _logger.LogInformation("PayOs webhook broadcasted via SignalR: SessionId={SessionId}, BookingId={BookingId}",
                    result.PaymentSessionId, result.BookingId);
            }

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            if (result.Success)
            {
                _logger.LogInformation("✅ [PAYOS-WEBHOOK-{WebhookId}] PayOs webhook processed successfully", webhookId);
                _logger.LogInformation("   BookingId: {BookingId}", result.BookingId);
                _logger.LogInformation("⏱️ Processing time: {Duration}ms", duration);
                _logger.LogInformation("═══════════════════════════════════════════════════════════");
                Console.WriteLine($"✅ [PAYOS-WEBHOOK-{webhookId}] SUCCESS! Booking {result.BookingId} processed ({duration:F0}ms)");
            }
            else
            {
                _logger.LogWarning("⚠️ [PAYOS-WEBHOOK-{WebhookId}] PayOs webhook failed: {Message}", 
                    webhookId, result.Message);
                Console.WriteLine($"❌ [PAYOS-WEBHOOK-{webhookId}] Failed: {result.Message}");
            }

            return result;
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "❌ [PAYOS-WEBHOOK-{WebhookId}] Error processing PayOs webhook after {Duration}ms", 
                webhookId, duration);
            Console.WriteLine($"❌ [PAYOS-WEBHOOK-{webhookId}] ERROR: {ex.Message}");
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            return new BankWebhookResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Verify signature từ PayOs
    /// PayOs sử dụng ChecksumKey để verify signature (HMAC-SHA256)
    /// </summary>
    private bool VerifySignature(PayOsWebhookDto dto, string? signature)
    {
        if (string.IsNullOrEmpty(signature))
        {
            var verifySignature = _configuration.GetValue<bool>("BankWebhook:PayOs:VerifySignature", true);
            if (!verifySignature)
            {
                _logger.LogWarning("PayOs signature verification is disabled");
                return true; // Development mode
            }
            return false;
        }

        // Ưu tiên dùng ChecksumKey, nếu không có thì dùng SecretKey
        var checksumKey = _configuration["BankWebhook:PayOs:ChecksumKey"];
        var secretKey = _configuration["BankWebhook:PayOs:SecretKey"];
        var keyToUse = !string.IsNullOrEmpty(checksumKey) ? checksumKey : secretKey;
        
        if (string.IsNullOrEmpty(keyToUse))
        {
            _logger.LogWarning("PayOs checksum key/secret key not configured");
            return false;
        }

        // PayOs signature format: HMAC-SHA256 của data
        // Tạo payload từ data object
        var dataStr = dto.Data != null 
            ? $"{dto.Data.TransactionId}{dto.Data.Amount}{dto.Data.Description}{dto.Data.AccountNumber}{dto.Code}"
            : $"{dto.Code}{dto.Desc}";
        
        // Compute HMAC-SHA256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keyToUse));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataStr));
        var computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        // Compare signatures
        var isValid = string.Equals(computedSignature, signature, StringComparison.OrdinalIgnoreCase);
        
        if (!isValid)
        {
            _logger.LogWarning("PayOs signature mismatch. Expected: {Expected}, Received: {Received}",
                computedSignature, signature);
        }

        return isValid;
    }
}

/// <summary>
/// DTO cho PayOs webhook
/// Format theo PayOs API documentation
/// </summary>
public class PayOsWebhookDto
{
    [JsonPropertyName("code")]
    public int Code { get; set; } // 0 = thành công, khác 0 = lỗi

    [JsonPropertyName("desc")]
    public string? Desc { get; set; } // Mô tả

    [JsonPropertyName("data")]
    public PayOsWebhookData? Data { get; set; } // Dữ liệu giao dịch

    [JsonPropertyName("signature")]
    public string? Signature { get; set; } // HMAC-SHA256 signature

    [JsonPropertyName("id")]
    public string? Id { get; set; } // PayOs transaction ID
}

/// <summary>
/// Dữ liệu giao dịch từ PayOs webhook
/// </summary>
public class PayOsWebhookData
{
    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; set; } // Transaction ID từ PayOs

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; } // Số tiền

    [JsonPropertyName("description")]
    public string? Description { get; set; } // Nội dung chuyển khoản (BOOKING-BKG2025039)

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; } // Số tài khoản nhận

    [JsonPropertyName("accountName")]
    public string? AccountName { get; set; } // Tên tài khoản nhận

    [JsonPropertyName("transactionDateTime")]
    public DateTime? TransactionDateTime { get; set; } // Thời gian giao dịch

    [JsonPropertyName("reference")]
    public string? Reference { get; set; } // Số tham chiếu

    [JsonPropertyName("status")]
    public string? Status { get; set; } // Trạng thái
}

