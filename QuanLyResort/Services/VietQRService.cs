using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuanLyResort.Services;

/// <summary>
/// Service để tạo QR code VietQR (Miễn phí)
/// Format: https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo={content}
/// </summary>
public class VietQRService
{
    private readonly ILogger<VietQRService> _logger;
    private readonly IConfiguration _configuration;

    // VietQR Configuration
    private readonly string? _bankCode;
    private readonly string? _bankAccountNumber;
    private readonly string? _bankAccountName;

    public VietQRService(
        ILogger<VietQRService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        // Load configuration từ environment variables hoặc appsettings.json
        // Hỗ trợ cả format cũ (SePay:*) và format mới (VietQR:*)
        
        // Bank Code (mặc định: MB)
        _bankCode = _configuration["VietQR:BankCode"]
                 ?? _configuration["SePay:BankCode"] // Fallback từ SePay config
                 ?? "MB";
        
        // Bank Account Number (bắt buộc)
        _bankAccountNumber = _configuration["VietQR:BankAccountNumber"]
                          ?? _configuration["SePay:BankAccountNumber"]; // Fallback từ SePay config
        
        // Bank Account Name (optional)
        _bankAccountName = _configuration["VietQR:BankAccountName"]
                       ?? _configuration["SePay:BankAccountName"]
                       ?? "Resort Deluxe";

        // Log configuration
        if (string.IsNullOrEmpty(_bankAccountNumber))
        {
            _logger.LogWarning("[VIETQR] ⚠️ Bank Account Number chưa được cấu hình. Vui lòng thêm 'VietQR:BankAccountNumber' hoặc 'SePay:BankAccountNumber' vào environment variables.");
        }
        else
        {
            _logger.LogInformation("[VIETQR] ✅ Service initialized with BankCode: {BankCode}, AccountNumber: {AccountNumber}", 
                _bankCode, MaskAccountNumber(_bankAccountNumber));
        }
    }

    /// <summary>
    /// Tạo QR code URL cho booking
    /// </summary>
    public string? CreateBookingQRCode(int bookingId, decimal amount)
    {
        try
        {
            if (string.IsNullOrEmpty(_bankAccountNumber))
            {
                _logger.LogError("[VIETQR] ❌ Bank Account Number chưa được cấu hình. Không thể tạo QR code.");
                return null;
            }

            var content = $"BOOKING{bookingId}";
            var qrCodeUrl = CreateQRCodeUrl(amount, content);
            
            _logger.LogInformation("[VIETQR] ✅ Tạo QR code cho booking {BookingId}: Amount={Amount:N0} VND, Content={Content}", 
                bookingId, amount, content);
            
            return qrCodeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VIETQR] ❌ Lỗi khi tạo QR code cho booking {BookingId}", bookingId);
            return null;
        }
    }

    /// <summary>
    /// Tạo QR code URL cho restaurant order
    /// </summary>
    public string? CreateRestaurantOrderQRCode(int orderId, decimal amount)
    {
        try
        {
            if (string.IsNullOrEmpty(_bankAccountNumber))
            {
                _logger.LogError("[VIETQR] ❌ Bank Account Number chưa được cấu hình. Không thể tạo QR code.");
                return null;
            }

            var content = $"ORDER{orderId}";
            var qrCodeUrl = CreateQRCodeUrl(amount, content);
            
            _logger.LogInformation("[VIETQR] ✅ Tạo QR code cho restaurant order {OrderId}: Amount={Amount:N0} VND, Content={Content}", 
                orderId, amount, content);
            
            return qrCodeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VIETQR] ❌ Lỗi khi tạo QR code cho restaurant order {OrderId}", orderId);
            return null;
        }
    }

    /// <summary>
    /// Tạo QR code URL từ VietQR
    /// Format: https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo={content}
    /// </summary>
    private string CreateQRCodeUrl(decimal amount, string content)
    {
        // URL encode các tham số
        var encodedContent = Uri.EscapeDataString(content);
        var bankCodeForUrl = _bankCode ?? "MB";
        var accountNumberForUrl = _bankAccountNumber ?? "";
        
        // Tạo QR code URL
        // Format: https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo={content}
        var qrCodeUrl = $"https://img.vietqr.io/image/{bankCodeForUrl}-{accountNumberForUrl}-compact2.png?amount={(long)amount}&addInfo={encodedContent}";
        
        _logger.LogDebug("[VIETQR] 🔍 QR Code URL: {Url}", qrCodeUrl);
        
        return qrCodeUrl;
    }

    /// <summary>
    /// Mask account number để log (chỉ hiển thị 4 số cuối)
    /// </summary>
    private string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length <= 4)
        {
            return "****";
        }
        return $"****{accountNumber.Substring(accountNumber.Length - 4)}";
    }

    /// <summary>
    /// Lấy bank account number
    /// </summary>
    public string? GetBankAccountNumber() => _bankAccountNumber;

    /// <summary>
    /// Lấy bank account name
    /// </summary>
    public string? GetBankAccountName() => _bankAccountName;

    /// <summary>
    /// Lấy bank code
    /// </summary>
    public string? GetBankCode() => _bankCode;
}

