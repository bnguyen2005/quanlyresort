using System.Security.Cryptography;
using System.Text;

namespace QuanLyResort.Services;

/// <summary>
/// Service xử lý webhook từ VietQR API
/// VietQR hỗ trợ nhiều ngân hàng (MB, VCB, TCB, etc.)
/// </summary>
public class VietQRWebhookService
{
    private readonly IBankWebhookService _bankWebhookService;
    private readonly ILogger<VietQRWebhookService> _logger;
    private readonly IConfiguration _configuration;

    public VietQRWebhookService(
        IBankWebhookService bankWebhookService,
        ILogger<VietQRWebhookService> logger,
        IConfiguration configuration)
    {
        _bankWebhookService = bankWebhookService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Xử lý webhook từ VietQR
    /// Format: https://docs.vietqr.io/
    /// </summary>
    public async Task<BankWebhookResult> ProcessVietQRWebhookAsync(Services.VietQRWebhookDto dto)
    {
        try
        {
            _logger.LogInformation("Processing VietQR webhook: TransactionId={TransactionId}, Amount={Amount}",
                dto.TransactionId, dto.Amount);

            // Verify signature
            var isValid = VerifySignature(dto, dto.Signature);
            if (!isValid)
            {
                _logger.LogWarning("Invalid VietQR webhook signature");
                return new BankWebhookResult
                {
                    Success = false,
                    Message = "Invalid signature"
                };
            }

            // Map VietQR format to BankWebhookRequest
            var bankRequest = new BankWebhookRequest
            {
                BankName = dto.BankCode ?? "MB", // Default to MB if not specified
                TransactionId = dto.TransactionId ?? string.Empty,
                Amount = dto.Amount,
                Content = dto.Content ?? string.Empty,
                AccountNumber = dto.AccountNumber ?? string.Empty,
                AccountName = dto.AccountName ?? string.Empty,
                TransactionDate = dto.TransactionDate ?? DateTime.UtcNow,
                Signature = dto.Signature,
                RawData = new Dictionary<string, object>
                {
                    { "vietqr_transaction_id", dto.VietQRTransactionId ?? string.Empty },
                    { "bank_code", dto.BankCode ?? "MB" },
                    { "bank_name", dto.BankName ?? "MBBank" }
                }
            };

            // Process through main webhook service
            return await _bankWebhookService.ProcessWebhookAsync(bankRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VietQR webhook");
            return new BankWebhookResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Verify signature từ VietQR
    /// VietQR sử dụng HMAC-SHA256 với secret key
    /// </summary>
    private bool VerifySignature(Services.VietQRWebhookDto dto, string? signature)
    {
        if (string.IsNullOrEmpty(signature))
        {
            // Nếu không có signature, có thể skip (development) hoặc reject (production)
            var verifySignature = _configuration.GetValue<bool>("BankWebhook:VietQR:VerifySignature", true);
            if (!verifySignature)
            {
                _logger.LogWarning("VietQR signature verification is disabled");
                return true; // Development mode
            }
            return false;
        }

        // Ưu tiên dùng ChecksumKey, nếu không có thì dùng SecretKey
        var checksumKey = _configuration["BankWebhook:VietQR:ChecksumKey"];
        var secretKey = _configuration["BankWebhook:VietQR:SecretKey"];
        var keyToUse = !string.IsNullOrEmpty(checksumKey) ? checksumKey : secretKey;
        
        if (string.IsNullOrEmpty(keyToUse))
        {
            _logger.LogWarning("VietQR checksum key/secret key not configured");
            return false;
        }

        // Tạo payload để hash
        // VietQR thường dùng format: transactionId + amount + content + accountNumber + timestamp
        var payload = $"{dto.TransactionId}{dto.Amount}{dto.Content}{dto.AccountNumber}{dto.TransactionDate:yyyyMMddHHmmss}";
        
        // Compute HMAC-SHA256 với ChecksumKey
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keyToUse));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        // Compare signatures
        var isValid = string.Equals(computedSignature, signature, StringComparison.OrdinalIgnoreCase);
        
        if (!isValid)
        {
            _logger.LogWarning("VietQR signature mismatch. Expected: {Expected}, Received: {Received}",
                computedSignature, signature);
        }

        return isValid;
    }
}

/// <summary>
/// DTO cho VietQR webhook
/// Format: https://docs.vietqr.io/webhook
/// </summary>
public class VietQRWebhookDto
{
    public string? TransactionId { get; set; } // Transaction ID từ ngân hàng
    public string? VietQRTransactionId { get; set; } // Transaction ID từ VietQR
    public decimal Amount { get; set; }
    public string? Content { get; set; } // Nội dung chuyển khoản
    public string? AccountNumber { get; set; } // Số tài khoản nhận
    public string? AccountName { get; set; } // Tên tài khoản nhận
    public string? BankCode { get; set; } // "MB", "VCB", "TCB", etc.
    public string? BankName { get; set; } // "MBBank", "Vietcombank", etc.
    public DateTime? TransactionDate { get; set; }
    public string? Signature { get; set; } // HMAC-SHA256 signature
    public string? Status { get; set; } // "success", "failed", etc.
}

