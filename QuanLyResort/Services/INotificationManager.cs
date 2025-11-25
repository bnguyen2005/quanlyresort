namespace QuanLyResort.Services;

public interface INotificationManager
{
    Task SendBookingConfirmationAsync(int customerId, string bookingCode, DateTime checkInDate, DateTime checkOutDate, decimal amount);
    Task SendPaymentConfirmationAsync(int customerId, string invoiceNumber, decimal amount, string paymentMethod);
    Task SendOrderConfirmationAsync(int customerId, string orderNumber, decimal amount);
    Task SendBookingCancellationAsync(int customerId, string bookingCode);
    Task SendOrderStatusUpdateAsync(int customerId, string orderNumber, string status);
    Task SendPaymentRequestAsync(int customerId, string invoiceNumber, decimal amount);
    Task SendAdminNotificationAsync(string title, string message, string? targetRole = null);
}

