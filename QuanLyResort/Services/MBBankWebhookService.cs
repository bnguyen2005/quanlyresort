using System.Security.Cryptography;
using System.Text;

namespace QuanLyResort.Services;

/// <summary>
/// Service xử lý webhook từ MB Bank (MBBank)
/// Có thể tích hợp qua:
/// 1. VietQR (đã hỗ trợ MB Bank)
/// 2. MB Bank Open Banking API (nếu có)
/// 3. MB Bank Merchant API (nếu có)
/// </summary>
public class MBBankWebhookService
{
    private readonly IBankWebhookService _bankWebhookService;
    private readonly ILogger<MBBankWebhookService> _logger;
    private readonly IConfiguration _configuration;

    public MBBankWebhookService(
        IBankWebhookService bankWebhookService,
        ILogger<MBBankWebhookService> logger,
        IConfiguration configuration)
    {
        _bankWebhookService = bankWebhookService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Xử lý webhook từ MB Bank
    /// Format có thể khác nhau tùy theo API của MB Bank
    /// </summary>
    public async Task<BankWebhookResult> ProcessMBBankWebhookAsync(Services.MBBankWebhookDto dto)
    {
        try
        {
            _logger.LogInformation("Processing MB Bank webhook: TransactionId={TransactionId}, Amount={Amount}",
                dto.TransactionId, dto.Amount);

            // Verify signature
            var isValid = VerifySignature(dto, dto.Signature);
            if (!isValid)
            {
                _logger.LogWarning("Invalid MB Bank webhook signature");
                return new BankWebhookResult
                {
                    Success = false,
                    Message = "Invalid signature"
                };
            }

            // Map MB Bank format to BankWebhookRequest
            var bankRequest = new BankWebhookRequest
            {
                BankName = "MB",
                TransactionId = dto.TransactionId ?? string.Empty,
                Amount = dto.Amount,
                Content = dto.Content ?? dto.TransactionDescription ?? string.Empty,
                AccountNumber = dto.AccountNumber ?? string.Empty,
                AccountName = dto.AccountName ?? "Resort Deluxe",
                TransactionDate = dto.TransactionDate ?? DateTime.UtcNow,
                Signature = dto.Signature,
                RawData = new Dictionary<string, object>
                {
                    { "mb_transaction_id", dto.MBTransactionId ?? string.Empty },
                    { "reference_number", dto.ReferenceNumber ?? string.Empty },
                    { "transaction_type", dto.TransactionType ?? "IN" }
                }
            };

            // Process through main webhook service
            return await _bankWebhookService.ProcessWebhookAsync(bankRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MB Bank webhook");
            return new BankWebhookResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Verify signature từ MB Bank
    /// MB Bank thường dùng HMAC-SHA256 hoặc RSA signature
    /// </summary>
    private bool VerifySignature(Services.MBBankWebhookDto dto, string? signature)
    {
        if (string.IsNullOrEmpty(signature))
        {
            var verifySignature = _configuration.GetValue<bool>("BankWebhook:MBBank:VerifySignature", true);
            if (!verifySignature)
            {
                _logger.LogWarning("MB Bank signature verification is disabled");
                return true; // Development mode
            }
            return false;
        }

        // Ưu tiên dùng SecretKey, nếu không có thì dùng ClientSecret
        var secretKey = _configuration["BankWebhook:MBBank:SecretKey"];
        var clientSecret = _configuration["BankWebhook:MBBank:ClientSecret"];
        var keyToUse = !string.IsNullOrEmpty(secretKey) ? secretKey : clientSecret;
        
        if (string.IsNullOrEmpty(keyToUse))
        {
            _logger.LogWarning("MB Bank secret key/client secret not configured");
            return false;
        }

        // Tạo payload để hash (format có thể khác tùy MB Bank API)
        var payload = $"{dto.TransactionId}{dto.Amount}{dto.Content}{dto.AccountNumber}{dto.TransactionDate:yyyyMMddHHmmss}";
        
        // Compute HMAC-SHA256 (MB Bank thường dùng)
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keyToUse));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        // Compare signatures
        var isValid = string.Equals(computedSignature, signature, StringComparison.OrdinalIgnoreCase);
        
        if (!isValid)
        {
            _logger.LogWarning("MB Bank signature mismatch. Expected: {Expected}, Received: {Received}",
                computedSignature, signature);
        }

        return isValid;
    }
}

/// <summary>
/// DTO cho MB Bank webhook
/// Format có thể khác nhau tùy theo API version của MB Bank
/// </summary>
public class MBBankWebhookDto
{
    public string? TransactionId { get; set; } // Transaction ID từ MB Bank
    public string? MBTransactionId { get; set; } // MB Bank internal transaction ID
    public decimal Amount { get; set; }
    public string? Content { get; set; } // Nội dung chuyển khoản
    public string? TransactionDescription { get; set; } // Mô tả giao dịch
    public string? AccountNumber { get; set; } // Số tài khoản nhận
    public string? AccountName { get; set; } // Tên tài khoản nhận
    public string? ReferenceNumber { get; set; } // Số tham chiếu
    public DateTime? TransactionDate { get; set; }
    public string? Signature { get; set; } // HMAC-SHA256 hoặc RSA signature
    public string? Status { get; set; } // "SUCCESS", "FAILED", etc.
    public string? TransactionType { get; set; } // "IN", "OUT", etc.
}

