using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyResort.Hubs;

/// <summary>
/// SignalR Hub cho real-time payment status updates
/// </summary>
[Authorize]
public class PaymentHub : Hub
{
    private readonly ILogger<PaymentHub> _logger;

    public PaymentHub(ILogger<PaymentHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Client join vào group của payment session
    /// </summary>
    public async Task JoinPaymentSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            await Clients.Caller.SendAsync("Error", "SessionId không hợp lệ");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"payment_{sessionId}");
        _logger.LogInformation("Client {ConnectionId} joined payment session {SessionId}", Context.ConnectionId, sessionId);
        
        await Clients.Caller.SendAsync("Joined", sessionId);
    }

    /// <summary>
    /// Client leave khỏi payment session group
    /// </summary>
    public async Task LeavePaymentSession(string sessionId)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"payment_{sessionId}");
            _logger.LogInformation("Client {ConnectionId} left payment session {SessionId}", Context.ConnectionId, sessionId);
        }
    }

    /// <summary>
    /// Client join vào group của booking (fallback nếu không có payment session)
    /// </summary>
    public async Task JoinBookingGroup(int bookingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"booking_{bookingId}");
        _logger.LogInformation("Client {ConnectionId} joined booking group {BookingId}", Context.ConnectionId, bookingId);
        await Clients.Caller.SendAsync("JoinedBooking", bookingId);
    }

    /// <summary>
    /// Client leave khỏi booking group
    /// </summary>
    public async Task LeaveBookingGroup(int bookingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"booking_{bookingId}");
        _logger.LogInformation("Client {ConnectionId} left booking group {BookingId}", Context.ConnectionId, bookingId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

