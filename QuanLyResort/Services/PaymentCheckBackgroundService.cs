using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuanLyResort.Services;

/// <summary>
/// Background service để tự động check payment status cho các booking đang pending
/// Chạy mỗi 10 giây để check các booking có status = "Pending" hoặc "Confirmed"
/// </summary>
public class PaymentCheckBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentCheckBackgroundService> _logger;

    public PaymentCheckBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PaymentCheckBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 PaymentCheckBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Tạo scope để inject services
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                // Lấy danh sách bookings đang pending (có thể thanh toán)
                // Note: Cần thêm method GetPendingBookingsAsync vào IBookingService
                // Tạm thời, service này sẽ không chạy cho đến khi có method đó
                
                // TODO: Implement logic để check payment từ PayOs API hoặc database
                // Hiện tại, service này chỉ log để không gây lỗi
                
                _logger.LogDebug("PaymentCheckBackgroundService: Checking payments...");
                
                // Đợi 10 giây trước khi check lại
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PaymentCheckBackgroundService");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Đợi lâu hơn nếu có lỗi
            }
        }

        _logger.LogInformation("🛑 PaymentCheckBackgroundService stopped");
    }
}

