namespace QuanLyResort.Services;

/// <summary>
/// Service để xử lý webhook từ các ngân hàng/API thanh toán
/// </summary>
public interface IBankWebhookService
{
    /// <summary>
    /// Xử lý webhook từ ngân hàng
    /// </summary>
    Task<BankWebhookResult> ProcessWebhookAsync(BankWebhookRequest request);
    
    /// <summary>
    /// Verify signature của webhook (để đảm bảo tính xác thực)
    /// </summary>
    Task<bool> VerifyWebhookSignatureAsync(BankWebhookRequest request, string signature);
    
    /// <summary>
    /// Parse booking code từ nội dung chuyển khoản
    /// Format: "BOOKING-BKG2025039" hoặc "BOOKING-39"
    /// </summary>
    int? ExtractBookingIdFromContent(string content);
}

/// <summary>
/// Request từ webhook ngân hàng
/// </summary>
public class BankWebhookRequest
{
    public string BankName { get; set; } = string.Empty; // "MB", "VCB", "TCB", "VietQR", etc.
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Content { get; set; } = string.Empty; // Nội dung chuyển khoản
    public string AccountNumber { get; set; } = string.Empty; // Số tài khoản nhận
    public string AccountName { get; set; } = string.Empty; // Tên tài khoản nhận
    public DateTime TransactionDate { get; set; }
    public string? Signature { get; set; } // Signature để verify
    public Dictionary<string, object>? RawData { get; set; } // Dữ liệu raw từ ngân hàng
}

/// <summary>
/// Kết quả xử lý webhook
/// </summary>
public class BankWebhookResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? BookingId { get; set; }
    public string? PaymentSessionId { get; set; }
    public bool BookingUpdated { get; set; }
}

