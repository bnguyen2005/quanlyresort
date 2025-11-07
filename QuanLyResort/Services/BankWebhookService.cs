using Microsoft.AspNetCore.SignalR;
using QuanLyResort.Hubs;

namespace QuanLyResort.Services;

/// <summary>
/// Service xử lý webhook từ các ngân hàng/API thanh toán
/// </summary>
public class BankWebhookService : IBankWebhookService
{
    private readonly IBookingService _bookingService;
    private readonly IPaymentSessionService _paymentSessionService;
    private readonly IHubContext<PaymentHub> _hubContext;
    private readonly ILogger<BankWebhookService> _logger;

    public BankWebhookService(
        IBookingService bookingService,
        IPaymentSessionService paymentSessionService,
        IHubContext<PaymentHub> hubContext,
        ILogger<BankWebhookService> logger)
    {
        _bookingService = bookingService;
        _paymentSessionService = paymentSessionService;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<BankWebhookResult> ProcessWebhookAsync(BankWebhookRequest request)
    {
        var webhookId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            _logger.LogInformation("📥 [BANK-WEBHOOK-{WebhookId}] Processing webhook from {BankName}", webhookId, request.BankName);
            _logger.LogInformation("   TransactionId: {TransactionId}", request.TransactionId);
            _logger.LogInformation("   Amount: {Amount:N0} VND", request.Amount);
            _logger.LogInformation("   Content: {Content}", request.Content);
            _logger.LogInformation("   AccountNumber: {AccountNumber}", request.AccountNumber);
            _logger.LogInformation("   TransactionDate: {TransactionDate}", request.TransactionDate);
            
            Console.WriteLine($"\n📥 [BANK-WEBHOOK-{webhookId}] {request.BankName}: {request.Content} - {request.Amount:N0} VND");

            // 1. Extract booking ID từ nội dung chuyển khoản
            _logger.LogInformation("🔍 [BANK-WEBHOOK-{WebhookId}] Extracting booking ID from content...", webhookId);
            var bookingId = ExtractBookingIdFromContent(request.Content);
            if (!bookingId.HasValue)
            {
                _logger.LogWarning("⚠️ [BANK-WEBHOOK-{WebhookId}] Could not extract booking ID from content: {Content}", webhookId, request.Content);
                Console.WriteLine($"⚠️ [BANK-WEBHOOK-{webhookId}] Cannot extract booking ID");
                return new BankWebhookResult
                {
                    Success = false,
                    Message = "Không tìm thấy booking ID trong nội dung chuyển khoản"
                };
            }

            // 2. Lấy booking để verify
            _logger.LogInformation("🔍 [BANK-WEBHOOK-{WebhookId}] Fetching booking {BookingId}...", webhookId, bookingId.Value);
            var booking = await _bookingService.GetBookingByIdAsync(bookingId.Value);
            if (booking == null)
            {
                _logger.LogWarning("⚠️ [BANK-WEBHOOK-{WebhookId}] Booking {BookingId} not found", webhookId, bookingId.Value);
                Console.WriteLine($"❌ [BANK-WEBHOOK-{webhookId}] Booking {bookingId.Value} not found");
                return new BankWebhookResult
                {
                    Success = false,
                    Message = $"Không tìm thấy booking ID {bookingId.Value}"
                };
            }

            _logger.LogInformation("✅ [BANK-WEBHOOK-{WebhookId}] Booking found: Code={BookingCode}, Status={Status}, Amount={Amount:N0} VND", 
                webhookId, booking.BookingCode, booking.Status, booking.EstimatedTotalAmount ?? 0);
            Console.WriteLine($"✅ [BANK-WEBHOOK-{webhookId}] Booking {booking.BookingCode} - Status: {booking.Status}");

            // 3. Kiểm tra booking đã được thanh toán chưa
            if (booking.Status == "Paid")
            {
                _logger.LogInformation("ℹ️ [BANK-WEBHOOK-{WebhookId}] Booking {BookingId} already paid, ignoring duplicate webhook", webhookId, bookingId.Value);
                Console.WriteLine($"ℹ️ [BANK-WEBHOOK-{webhookId}] Booking already paid - ignoring");
                return new BankWebhookResult
                {
                    Success = true,
                    Message = "Booking đã được thanh toán trước đó",
                    BookingId = bookingId.Value,
                    BookingUpdated = false
                };
            }

            // 4. Verify amount (có thể cho phép sai số nhỏ)
            _logger.LogInformation("🔍 [BANK-WEBHOOK-{WebhookId}] Verifying amount...", webhookId);
            var expectedAmount = booking.EstimatedTotalAmount ?? 0;
            var amountDifference = Math.Abs((double)(request.Amount - expectedAmount));
            var tolerance = 0.01m; // Cho phép sai số 0.01 VND

            if (amountDifference > (double)tolerance)
            {
                _logger.LogWarning("⚠️ [BANK-WEBHOOK-{WebhookId}] Amount mismatch for booking {BookingId}. Expected: {Expected}, Received: {Received}",
                    webhookId, bookingId.Value, expectedAmount, request.Amount);
                Console.WriteLine($"⚠️ [BANK-WEBHOOK-{webhookId}] Amount mismatch: Expected {expectedAmount:N0}, Received {request.Amount:N0}");
                // Có thể vẫn chấp nhận nếu amount lớn hơn expected (khách chuyển thừa)
                if (request.Amount < expectedAmount)
                {
                    return new BankWebhookResult
                    {
                        Success = false,
                        Message = $"Số tiền không khớp. Mong đợi: {expectedAmount}, Nhận được: {request.Amount}",
                        BookingId = bookingId.Value
                    };
                }
            }

            // 5. Tìm payment session liên quan
            var sessions = await _paymentSessionService.GetSessionsByBookingIdAsync(bookingId.Value);
            var activeSession = sessions?.FirstOrDefault(s => 
                s.Status == PaymentStatus.Pending || 
                s.Status == PaymentStatus.Processing);

            // 6. Cập nhật payment session nếu có
            string? sessionIdToBroadcast = null;
            if (activeSession != null)
            {
                await _paymentSessionService.UpdateSessionStatusAsync(
                    activeSession.SessionId,
                    PaymentStatus.Paid,
                    request.TransactionId,
                    $"INV-{booking.BookingCode}",
                    null);

                sessionIdToBroadcast = activeSession.SessionId;
                _logger.LogInformation("Payment session {SessionId} updated to Paid", activeSession.SessionId);
            }

            // 7. Cập nhật booking status TRƯỚC khi broadcast SignalR
            _logger.LogInformation("🔄 [BANK-WEBHOOK-{WebhookId}] Updating booking {BookingId} to Paid status...", webhookId, bookingId.Value);
            var performedBy = $"BankWebhook-{request.BankName}-{request.TransactionId}";
            var paymentSuccess = await _bookingService.ProcessOnlinePaymentAsync(bookingId.Value, performedBy);

            if (!paymentSuccess)
            {
                _logger.LogError("❌ [BANK-WEBHOOK-{WebhookId}] Failed to process payment for booking {BookingId}", webhookId, bookingId.Value);
                Console.WriteLine($"❌ [BANK-WEBHOOK-{webhookId}] Failed to update booking");
                return new BankWebhookResult
                {
                    Success = false,
                    Message = "Không thể cập nhật booking",
                    BookingId = bookingId.Value
                };
            }

            // 8. Broadcast qua SignalR cho TẤT CẢ sessions của booking này (nếu có)
            // Và broadcast cả cho booking group (fallback nếu không có session)
            var allSessions = await _paymentSessionService.GetSessionsByBookingIdAsync(bookingId.Value);
            var broadcastTasks = new List<Task>();

            // Broadcast cho từng active session
            foreach (var session in allSessions.Where(s => s.Status == PaymentStatus.Paid || s.Status == PaymentStatus.Pending))
            {
                broadcastTasks.Add(_hubContext.Clients.Group($"payment_{session.SessionId}").SendAsync("PaymentStatusChanged", new
                {
                    sessionId = session.SessionId,
                    bookingId = bookingId.Value,
                    status = "paid",
                    transactionId = request.TransactionId,
                    invoiceNumber = $"INV-{booking.BookingCode}",
                    paidAt = request.TransactionDate,
                    errorMessage = (string?)null
                }));
            }

            // Broadcast cho booking group (fallback cho các client không có session)
            broadcastTasks.Add(_hubContext.Clients.Group($"booking_{bookingId.Value}").SendAsync("BookingStatusChanged", new
            {
                bookingId = bookingId.Value,
                status = "Paid",
                transactionId = request.TransactionId,
                paidAt = request.TransactionDate
            }));

            // Wait for all broadcasts
            await Task.WhenAll(broadcastTasks);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("✅ [BANK-WEBHOOK-{WebhookId}] Successfully processed payment for booking {BookingId} from {BankName}", 
                webhookId, bookingId.Value, request.BankName);
            _logger.LogInformation("   TransactionId: {TransactionId}", request.TransactionId);
            _logger.LogInformation("   Broadcasted to {SessionCount} sessions", allSessions.Count);
            _logger.LogInformation("⏱️ Processing time: {Duration}ms", duration);
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            
            Console.WriteLine($"✅ [BANK-WEBHOOK-{webhookId}] SUCCESS! Booking {bookingId.Value} updated ({duration:F0}ms)");

            return new BankWebhookResult
            {
                Success = true,
                Message = "Thanh toán được xử lý thành công",
                BookingId = bookingId.Value,
                PaymentSessionId = sessionIdToBroadcast ?? allSessions.FirstOrDefault()?.SessionId,
                BookingUpdated = true
            };
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "❌ [BANK-WEBHOOK-{WebhookId}] Error processing bank webhook after {Duration}ms", webhookId, duration);
            Console.WriteLine($"❌ [BANK-WEBHOOK-{webhookId}] ERROR: {ex.Message}");
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            return new BankWebhookResult
            {
                Success = false,
                Message = $"Lỗi xử lý webhook: {ex.Message}"
            };
        }
    }

    public async Task<bool> VerifyWebhookSignatureAsync(BankWebhookRequest request, string signature)
    {
        // TODO: Implement signature verification dựa trên ngân hàng
        // Ví dụ:
        // - VietQR: HMAC-SHA256 với secret key
        // - VNPay: HMAC-SHA512 với secret key
        // - Các ngân hàng khác: theo documentation của họ

        // Hiện tại chỉ log warning, production cần implement đầy đủ
        _logger.LogWarning("Signature verification not implemented for {BankName}", request.BankName);
        return true; // Tạm thời return true, production cần verify thật
    }

    public int? ExtractBookingIdFromContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        // Format: "BOOKING-BKG2025039" hoặc "BOOKING-39" hoặc "BOOKING-BKG39"
        // Case insensitive
        var upperContent = content.ToUpper().Trim();

        // Pattern 1: "BOOKING-BKG2025039" hoặc "BOOKING-BKG39"
        var pattern1 = @"BOOKING[-_]?BKG(\d+)";
        var match1 = System.Text.RegularExpressions.Regex.Match(upperContent, pattern1);
        if (match1.Success && match1.Groups.Count > 1)
        {
            if (int.TryParse(match1.Groups[1].Value, out var bookingId))
            {
                return bookingId;
            }
        }

        // Pattern 2: "BOOKING-39" (chỉ số)
        var pattern2 = @"BOOKING[-_]?(\d+)";
        var match2 = System.Text.RegularExpressions.Regex.Match(upperContent, pattern2);
        if (match2.Success && match2.Groups.Count > 1)
        {
            if (int.TryParse(match2.Groups[1].Value, out var bookingId))
            {
                return bookingId;
            }
        }

        // Pattern 3: Chỉ có số booking ID (nếu content chỉ có số)
        if (int.TryParse(upperContent, out var directBookingId))
        {
            // Chỉ accept nếu số hợp lý (ví dụ từ 1-999999)
            if (directBookingId > 0 && directBookingId < 1000000)
            {
                return directBookingId;
            }
        }

        return null;
    }
}

