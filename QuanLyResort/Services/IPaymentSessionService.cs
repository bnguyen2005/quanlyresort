namespace QuanLyResort.Services;

public enum PaymentStatus
{
    Pending,      // Chờ thanh toán
    Processing,   // Đang xử lý
    Paid,         // Đã thanh toán
    Failed,       // Thất bại
    Expired,      // Hết hạn
    Cancelled     // Đã hủy
}

public class PaymentSession
{
    public string SessionId { get; set; } = string.Empty;
    public int BookingId { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TransactionId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IPaymentSessionService
{
    /// <summary>
    /// Tạo payment session mới
    /// </summary>
    Task<PaymentSession> CreateSessionAsync(int bookingId, int customerId, decimal amount, int expiryMinutes = 15);

    /// <summary>
    /// Lấy session theo sessionId
    /// </summary>
    Task<PaymentSession?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Cập nhật trạng thái payment session
    /// </summary>
    Task<bool> UpdateSessionStatusAsync(string sessionId, PaymentStatus status, string? transactionId = null, string? invoiceNumber = null, string? errorMessage = null);

    /// <summary>
    /// Kiểm tra session có hết hạn không
    /// </summary>
    Task<bool> IsSessionExpiredAsync(string sessionId);

    /// <summary>
    /// Xóa session (cleanup)
    /// </summary>
    Task<bool> DeleteSessionAsync(string sessionId);

    /// <summary>
    /// Lấy tất cả sessions của một booking
    /// </summary>
    Task<List<PaymentSession>> GetSessionsByBookingIdAsync(int bookingId);
}

