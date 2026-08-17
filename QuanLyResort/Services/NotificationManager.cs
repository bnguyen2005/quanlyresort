using QuanLyResort.Repositories;
using Microsoft.Extensions.Logging;
using QuanLyResort.Data;
using QuanLyResort.Models;

namespace QuanLyResort.Services;

public class NotificationManager : INotificationManager
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationManager> _logger;

    public NotificationManager(
        IEmailService emailService,
        ISmsService smsService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger<NotificationManager> logger)
    {
        _emailService = emailService;
        _smsService = smsService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SendBookingConfirmationAsync(int customerId, string bookingCode, DateTime checkInDate, DateTime checkOutDate, decimal amount)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return;

            var formattedAmount = amount.ToString("N0") + " ₫";
            var title = "🎉 Đặt phòng thành công!";
            var message = $"Mã đặt phòng: {bookingCode}\n" +
                         $"Ngày nhận phòng: {checkInDate:dd/MM/yyyy}\n" +
                         $"Ngày trả phòng: {checkOutDate:dd/MM/yyyy}\n" +
                         $"Tổng tiền: {formattedAmount}";

            // Send email
            var emailBody = GenerateBookingConfirmationEmail(customer.FullName, bookingCode, checkInDate, checkOutDate, amount);
            await _emailService.SendEmailAsync(customer.Email, title, emailBody, true);

            // SMS disabled - removed

            // Create in-app notification
            await _notificationService.CreateNotificationAsync(
                "BookingConfirmation",
                title,
                message,
                "Success",
                null,
                customerId,
                "Booking",
                null
            );

            _logger.LogInformation("[NotificationManager] ✅ Booking confirmation sent to customer {CustomerId}", customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotificationManager] ❌ Error sending booking confirmation: {Message}", ex.Message);
        }
    }

    public async Task SendPaymentConfirmationAsync(int customerId, string invoiceNumber, decimal amount, string paymentMethod)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return;

            var formattedAmount = amount.ToString("N0") + " ₫";
            var title = "✅ Thanh toán thành công!";
            var message = $"Mã hóa đơn: {invoiceNumber}\n" +
                         $"Số tiền: {formattedAmount}\n" +
                         $"Phương thức: {paymentMethod}";

            // Send email
            var emailBody = GeneratePaymentConfirmationEmail(customer.FullName, invoiceNumber, amount, paymentMethod);
            await _emailService.SendEmailAsync(customer.Email, title, emailBody, true);

            // SMS disabled - removed

            // Create in-app notification
            await _notificationService.CreateNotificationAsync(
                "PaymentConfirmation",
                title,
                message,
                "Success",
                null,
                customerId,
                "Invoice",
                null
            );

            _logger.LogInformation("[NotificationManager] ✅ Payment confirmation sent to customer {CustomerId}", customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotificationManager] ❌ Error sending payment confirmation: {Message}", ex.Message);
        }
    }

    public async Task SendOrderConfirmationAsync(int customerId, string orderNumber, decimal amount)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return;

            var formattedAmount = amount.ToString("N0") + " ₫";
            var title = "🍽️ Đặt món thành công!";
            var message = $"Mã đơn: {orderNumber}\n" +
                         $"Tổng tiền: {formattedAmount}";

            // Send email
            var emailBody = GenerateOrderConfirmationEmail(customer.FullName, orderNumber, amount);
            await _emailService.SendEmailAsync(customer.Email, title, emailBody, true);

            // SMS disabled - removed

            // Create in-app notification
            await _notificationService.CreateNotificationAsync(
                "OrderConfirmation",
                title,
                message,
                "Success",
                null,
                customerId,
                "RestaurantOrder",
                null
            );

            _logger.LogInformation("[NotificationManager] ✅ Order confirmation sent to customer {CustomerId}", customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotificationManager] ❌ Error sending order confirmation: {Message}", ex.Message);
        }
    }

    public async Task SendBookingCancellationAsync(int customerId, string bookingCode)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return;

            var title = "❌ Đặt phòng đã bị hủy";
            var message = $"Mã đặt phòng: {bookingCode}\n" +
                         $"Đặt phòng của bạn đã được hủy thành công.";

            // Send email
            var emailBody = GenerateBookingCancellationEmail(customer.FullName, bookingCode);
            await _emailService.SendEmailAsync(customer.Email, title, emailBody, true);

            // Create in-app notification
            await _notificationService.CreateNotificationAsync(
                "BookingCancellation",
                title,
                message,
                "Warning",
                null,
                customerId,
                "Booking",
                null
            );

            _logger.LogInformation("[NotificationManager] ✅ Booking cancellation sent to customer {CustomerId}", customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotificationManager] ❌ Error sending booking cancellation: {Message}", ex.Message);
        }
    }

    public async Task SendOrderStatusUpdateAsync(int customerId, string orderNumber, string status)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return;

            var statusText = status switch
            {
                "Confirmed" => "Đã xác nhận",
                "Preparing" => "Đang chuẩn bị",
                "Ready" => "Sẵn sàng",
                "Completed" => "Hoàn thành",
                _ => status
            };

            var title = $"📦 Cập nhật đơn hàng: {statusText}";
            var message = $"Mã đơn: {orderNumber}\n" +
                         $"Trạng thái: {statusText}";

            // Create in-app notification
            await _notificationService.CreateNotificationAsync(
                "OrderStatusUpdate",
                title,
                message,
                "Info",
                null,
                customerId,
                "RestaurantOrder",
                null
            );

            _logger.LogInformation("[NotificationManager] ✅ Order status update sent to customer {CustomerId}", customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotificationManager] ❌ Error sending order status update: {Message}", ex.Message);
        }
    }

    public async Task SendPaymentRequestAsync(int customerId, string invoiceNumber, decimal amount)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return;

            var formattedAmount = amount.ToString("N0") + " ₫";
            var title = "💳 Yêu cầu thanh toán";
            var message = $"Mã hóa đơn: {invoiceNumber}\n" +
                         $"Số tiền cần thanh toán: {formattedAmount}";

            // Send email
            var emailBody = GeneratePaymentRequestEmail(customer.FullName, invoiceNumber, amount);
            await _emailService.SendEmailAsync(customer.Email, title, emailBody, true);

            // Create in-app notification
            await _notificationService.CreateNotificationAsync(
                "PaymentRequest",
                title,
                message,
                "Info",
                null,
                customerId,
                "Invoice",
                null
            );

            _logger.LogInformation("[NotificationManager] ✅ Payment request sent to customer {CustomerId}", customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotificationManager] ❌ Error sending payment request: {Message}", ex.Message);
        }
    }

    public async Task SendAdminNotificationAsync(string title, string message, string? targetRole = null)
    {
        try
        {
            await _notificationService.CreateNotificationAsync(
                "AdminNotification",
                title,
                message,
                "Info",
                targetRole,
                null,
                null,
                null
            );

            _logger.LogInformation("[NotificationManager] ✅ Admin notification sent to role {Role}", targetRole ?? "All");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotificationManager] ❌ Error sending admin notification: {Message}", ex.Message);
        }
    }

    // Email templates
    private string GenerateBookingConfirmationEmail(string customerName, string bookingCode, DateTime checkInDate, DateTime checkOutDate, decimal amount)
    {
        var formattedAmount = amount.ToString("N0") + " ₫";
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #c8a97e 0%, #b89968 100%); color: white; padding: 30px; border-radius: 8px 8px 0 0; text-align: center; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px; }}
        .info-box {{ background: white; padding: 20px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #c8a97e; }}
        .label {{ font-weight: bold; color: #c8a97e; }}
        .amount {{ font-size: 24px; color: #059669; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1 style=""margin: 0;"">🎉 Đặt phòng thành công!</h1>
        </div>
        <div class=""content"">
            <p>Xin chào <strong>{customerName}</strong>,</p>
            <p>Cảm ơn bạn đã đặt phòng tại Resort Deluxe!</p>
            
            <div class=""info-box"">
                <div><span class=""label"">Mã đặt phòng:</span> <strong>{bookingCode}</strong></div>
                <div style=""margin-top: 10px;""><span class=""label"">Ngày nhận phòng:</span> {checkInDate:dd/MM/yyyy}</div>
                <div style=""margin-top: 10px;""><span class=""label"">Ngày trả phòng:</span> {checkOutDate:dd/MM/yyyy}</div>
                <div style=""margin-top: 15px; padding-top: 15px; border-top: 1px solid #eee;"">
                    <span class=""label"">Tổng tiền:</span> <span class=""amount"">{formattedAmount}</span>
                </div>
            </div>
            
            <p>Chúng tôi rất mong được phục vụ bạn!</p>
            <p>Trân trọng,<br>Đội ngũ Resort Deluxe</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GeneratePaymentConfirmationEmail(string customerName, string invoiceNumber, decimal amount, string paymentMethod)
    {
        var formattedAmount = amount.ToString("N0") + " ₫";
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #059669 0%, #047857 100%); color: white; padding: 30px; border-radius: 8px 8px 0 0; text-align: center; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px; }}
        .info-box {{ background: white; padding: 20px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #059669; }}
        .label {{ font-weight: bold; color: #059669; }}
        .amount {{ font-size: 24px; color: #059669; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1 style=""margin: 0;"">✅ Thanh toán thành công!</h1>
        </div>
        <div class=""content"">
            <p>Xin chào <strong>{customerName}</strong>,</p>
            <p>Thanh toán của bạn đã được xử lý thành công!</p>
            
            <div class=""info-box"">
                <div><span class=""label"">Mã hóa đơn:</span> <strong>{invoiceNumber}</strong></div>
                <div style=""margin-top: 10px;""><span class=""label"">Phương thức thanh toán:</span> {paymentMethod}</div>
                <div style=""margin-top: 15px; padding-top: 15px; border-top: 1px solid #eee;"">
                    <span class=""label"">Số tiền:</span> <span class=""amount"">{formattedAmount}</span>
                </div>
            </div>
            
            <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
            <p>Trân trọng,<br>Đội ngũ Resort Deluxe</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateOrderConfirmationEmail(string customerName, string orderNumber, decimal amount)
    {
        var formattedAmount = amount.ToString("N0") + " ₫";
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); color: white; padding: 30px; border-radius: 8px 8px 0 0; text-align: center; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px; }}
        .info-box {{ background: white; padding: 20px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #f59e0b; }}
        .label {{ font-weight: bold; color: #f59e0b; }}
        .amount {{ font-size: 24px; color: #f59e0b; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1 style=""margin: 0;"">🍽️ Đặt món thành công!</h1>
        </div>
        <div class=""content"">
            <p>Xin chào <strong>{customerName}</strong>,</p>
            <p>Cảm ơn bạn đã đặt món tại nhà hàng của chúng tôi!</p>
            
            <div class=""info-box"">
                <div><span class=""label"">Mã đơn hàng:</span> <strong>{orderNumber}</strong></div>
                <div style=""margin-top: 15px; padding-top: 15px; border-top: 1px solid #eee;"">
                    <span class=""label"">Tổng tiền:</span> <span class=""amount"">{formattedAmount}</span>
                </div>
            </div>
            
            <p>Đơn hàng của bạn đang được chuẩn bị. Chúng tôi sẽ thông báo khi sẵn sàng!</p>
            <p>Trân trọng,<br>Đội ngũ Resort Deluxe</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateBookingCancellationEmail(string customerName, string bookingCode)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); color: white; padding: 30px; border-radius: 8px 8px 0 0; text-align: center; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px; }}
        .info-box {{ background: white; padding: 20px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #ef4444; }}
        .label {{ font-weight: bold; color: #ef4444; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1 style=""margin: 0;"">❌ Đặt phòng đã bị hủy</h1>
        </div>
        <div class=""content"">
            <p>Xin chào <strong>{customerName}</strong>,</p>
            <p>Đặt phòng của bạn đã được hủy thành công.</p>
            
            <div class=""info-box"">
                <div><span class=""label"">Mã đặt phòng:</span> <strong>{bookingCode}</strong></div>
            </div>
            
            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi.</p>
            <p>Trân trọng,<br>Đội ngũ Resort Deluxe</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GeneratePaymentRequestEmail(string customerName, string invoiceNumber, decimal amount)
    {
        var formattedAmount = amount.ToString("N0") + " ₫";
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%); color: white; padding: 30px; border-radius: 8px 8px 0 0; text-align: center; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px; }}
        .info-box {{ background: white; padding: 20px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #3b82f6; }}
        .label {{ font-weight: bold; color: #3b82f6; }}
        .amount {{ font-size: 24px; color: #3b82f6; font-weight: bold; }}
        .button {{ display: inline-block; padding: 12px 24px; background: #3b82f6; color: white; text-decoration: none; border-radius: 6px; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1 style=""margin: 0;"">💳 Yêu cầu thanh toán</h1>
        </div>
        <div class=""content"">
            <p>Xin chào <strong>{customerName}</strong>,</p>
            <p>Bạn có một hóa đơn cần thanh toán:</p>
            
            <div class=""info-box"">
                <div><span class=""label"">Mã hóa đơn:</span> <strong>{invoiceNumber}</strong></div>
                <div style=""margin-top: 15px; padding-top: 15px; border-top: 1px solid #eee;"">
                    <span class=""label"">Số tiền cần thanh toán:</span> <span class=""amount"">{formattedAmount}</span>
                </div>
            </div>
            
            <p>Vui lòng thanh toán để hoàn tất đặt phòng của bạn.</p>
            <p>Trân trọng,<br>Đội ngũ Resort Deluxe</p>
        </div>
    </div>
</body>
</html>";
    }
}


