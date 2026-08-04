using System.Collections.Concurrent;
using QuanLyResort.Services;

namespace QuanLyResort.Services;

/// <summary>
/// In-memory payment session service (có thể migrate sang Redis/Database sau)
/// </summary>
public class PaymentSessionService : IPaymentSessionService
{
    private readonly ConcurrentDictionary<string, PaymentSession> _sessions = new();
    private readonly ILogger<PaymentSessionService> _logger;
    private readonly Timer _cleanupTimer;

    public PaymentSessionService(ILogger<PaymentSessionService> logger)
    {
        _logger = logger;
        
        // Cleanup expired sessions mỗi 5 phút
        _cleanupTimer = new Timer(CleanupExpiredSessions, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public Task<PaymentSession> CreateSessionAsync(int bookingId, int customerId, decimal amount, int expiryMinutes = 15)
    {
        // Tạo sessionId khó đoán (GUID + timestamp hash)
        var sessionId = GenerateSecureSessionId();
        
        var session = new PaymentSession
        {
            SessionId = sessionId,
            BookingId = bookingId,
            CustomerId = customerId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };

        _sessions[sessionId] = session;
        _logger.LogInformation("Created payment session {SessionId} for booking {BookingId}, amount: {Amount}", 
            sessionId, bookingId, amount);

        return Task.FromResult(session);
    }

    public Task<PaymentSession?> GetSessionAsync(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            // Kiểm tra hết hạn
            if (session.ExpiresAt.HasValue && session.ExpiresAt.Value < DateTime.UtcNow)
            {
                session.Status = PaymentStatus.Expired;
                _logger.LogWarning("Session {SessionId} has expired", sessionId);
            }
            
            return Task.FromResult<PaymentSession?>(session);
        }

        return Task.FromResult<PaymentSession?>(null);
    }

    public Task<bool> UpdateSessionStatusAsync(string sessionId, PaymentStatus status, string? transactionId = null, string? invoiceNumber = null, string? errorMessage = null)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            _logger.LogWarning("Session {SessionId} not found for update", sessionId);
            return Task.FromResult(false);
        }

        var oldStatus = session.Status;
        session.Status = status;
        
        if (status == PaymentStatus.Paid)
        {
            session.PaidAt = DateTime.UtcNow;
            session.TransactionId = transactionId;
            session.InvoiceNumber = invoiceNumber;
        }
        else if (status == PaymentStatus.Failed)
        {
            session.ErrorMessage = errorMessage;
        }

        _logger.LogInformation("Updated session {SessionId} status from {OldStatus} to {NewStatus}", 
            sessionId, oldStatus, status);

        return Task.FromResult(true);
    }

    public async Task<bool> IsSessionExpiredAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null) return true;
        
        if (session.Status == PaymentStatus.Expired) return true;
        if (session.ExpiresAt.HasValue && session.ExpiresAt.Value < DateTime.UtcNow)
        {
            await UpdateSessionStatusAsync(sessionId, PaymentStatus.Expired);
            return true;
        }
        
        return false;
    }

    public Task<bool> DeleteSessionAsync(string sessionId)
    {
        var removed = _sessions.TryRemove(sessionId, out _);
        if (removed)
        {
            _logger.LogInformation("Deleted session {SessionId}", sessionId);
        }
        return Task.FromResult(removed);
    }

    public Task<List<PaymentSession>> GetSessionsByBookingIdAsync(int bookingId)
    {
        var sessions = _sessions.Values
            .Where(s => s.BookingId == bookingId)
            .ToList();
        
        return Task.FromResult(sessions);
    }

    private string GenerateSecureSessionId()
    {
        // Tạo sessionId khó đoán: GUID + timestamp hash
        var guid = Guid.NewGuid().ToString("N");
        var timestamp = DateTime.UtcNow.Ticks;
        var hash = (guid + timestamp).GetHashCode().ToString("X");
        return $"{guid.Substring(0, 8)}{hash.Substring(0, 8)}";
    }

    private void CleanupExpiredSessions(object? state)
    {
        var expiredSessions = _sessions.Values
            .Where(s => s.ExpiresAt.HasValue && s.ExpiresAt.Value < DateTime.UtcNow)
            .ToList();

        foreach (var session in expiredSessions)
        {
            session.Status = PaymentStatus.Expired;
            _logger.LogInformation("Marked expired session {SessionId} as Expired", session.SessionId);
        }
    }
}

