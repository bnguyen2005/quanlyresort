namespace QuanLyResort.Services;

public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
    Task<bool> SendBookingConfirmationSmsAsync(string phoneNumber, string bookingCode, DateTime checkInDate, DateTime checkOutDate);
    Task<bool> SendPaymentConfirmationSmsAsync(string phoneNumber, string invoiceNumber, decimal amount);
    Task<bool> SendOrderConfirmationSmsAsync(string phoneNumber, string orderNumber, decimal amount);
}

