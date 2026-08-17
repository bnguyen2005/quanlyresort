using QuanLyResort.Repositories;
using Microsoft.AspNetCore.Mvc;
using QuanLyResort.Services;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json.Serialization;
using QuanLyResort.Data;
using Microsoft.EntityFrameworkCore;

namespace QuanLyResort.Controllers;

/// <summary>
/// Controller đơn giản cho thanh toán - tạo PayOs payment link và xử lý webhook
/// </summary>
[ApiController]
    [Route("api/simplepayment")]
    public class BookingPaymentController : ControllerBase
    {
    private readonly IBookingService _bookingService;
    private readonly PayOsService _payOsService;
    private readonly SePayService? _sePayService;
    private readonly VietQRService? _vietQRService;
    private readonly ILogger<BookingPaymentController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public BookingPaymentController(
        IBookingService bookingService,
        PayOsService payOsService,
        SePayService? sePayService,
        VietQRService? vietQRService,
        ILogger<BookingPaymentController> logger,
        IUnitOfWork unitOfWork)
    {
        _bookingService = bookingService;
        _payOsService = payOsService;
        _sePayService = sePayService;
        _vietQRService = vietQRService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Tạo PayOs payment link
    /// </summary>
    [HttpPost("create-link")]
    [Authorize]
    public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentLinkRequest request)
    {
        try
        {
            _logger.LogInformation("[BACKEND] 🔄 [CreateLink] Creating PayOs payment link for booking {BookingId}", request.BookingId);

            // Get booking
            var booking = await _bookingService.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
            {
                return NotFound(new { message = $"Booking {request.BookingId} không tồn tại" });
            }

            // Check if already paid
            if (booking.Status == "Paid")
            {
                return BadRequest(new { message = "Đặt phòng này đã được thanh toán" });
            }

            // Get amount
            var amount = booking.EstimatedTotalAmount ?? 0;
            if (amount <= 0)
            {
                return BadRequest(new { message = "Số tiền thanh toán không hợp lệ" });
            }

            // Tạo orderCode unique để tránh conflict với PayOs
            // PayOs yêu cầu orderCode phải unique, nếu bookingId trùng sẽ báo lỗi "đã tồn tại"
            // Giải pháp: orderCode = bookingId * 10000 + timestamp (giây) để đảm bảo unique
            var timestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            var orderCode = request.BookingId * 10000L + (timestamp % 10000); // Đảm bảo unique
            var description = $"BOOKING{request.BookingId}"; // PayOs description
            
            // Get base URL from request
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var returnUrl = $"{baseUrl}/customer/my-bookings.html?payment=success&bookingId={request.BookingId}";
            var cancelUrl = $"{baseUrl}/customer/my-bookings.html?payment=cancelled&bookingId={request.BookingId}";

            // Create payment link via PayOs API
            var expiredAt = DateTime.UtcNow.AddHours(24); // Expire after 24 hours
            var paymentLink = await _payOsService.CreatePaymentLinkAsync(
                orderCode: orderCode,
                amount: amount,
                description: description,
                returnUrl: returnUrl,
                cancelUrl: cancelUrl,
                expiredAt: expiredAt
            );

            if (paymentLink == null)
            {
                _logger.LogError("[BACKEND] ❌ [CreateLink] PayOs service returned null");
                return StatusCode(500, new { 
                    message = "Không thể tạo mã thanh toán. Vui lòng thử lại.",
                    error = "PayOs service returned null"
                });
            }

            if (paymentLink.Data == null)
            {
                // Nếu lỗi "Đơn thanh toán đã tồn tại", thử lấy payment link hiện có
                if (paymentLink.Desc?.Contains("đã tồn tại") == true || 
                    paymentLink.Desc?.Contains("already exists") == true ||
                    paymentLink.Code == "03")
                {
                    _logger.LogWarning("[BACKEND] ⚠️ [CreateLink] Payment link already exists for orderCode {OrderCode}. Trying to get existing link...", orderCode);
                    
                    var existingLink = await _payOsService.GetPaymentLinkByOrderCodeAsync(orderCode);
                    if (existingLink?.Data != null)
                    {
                        _logger.LogInformation("[BACKEND] ✅ [CreateLink] Found existing payment link: PaymentLinkId={PaymentLinkId}", 
                            existingLink.Data.PaymentLinkId);
                        
                        // Trả về payment link hiện có
                        return Ok(new
                        {
                            success = true,
                            paymentLinkId = existingLink.Data.PaymentLinkId,
                            orderCode = existingLink.Data.OrderCode,
                            qrCode = existingLink.Data.QrCode,
                            checkoutUrl = existingLink.Data.CheckoutUrl,
                            amount = existingLink.Data.Amount,
                            description = existingLink.Data.Description,
                            accountNumber = existingLink.Data.AccountNumber,
                            accountName = existingLink.Data.AccountName,
                            expiredAt = existingLink.Data.ExpiredAt
                        });
                    }
                }
                
                _logger.LogError("[BACKEND] ❌ [CreateLink] PayOs returned error. Code: {Code}, Desc: {Desc}", 
                    paymentLink.Code, paymentLink.Desc);
                return StatusCode(500, new { 
                    message = $"Không thể tạo mã thanh toán. {paymentLink.Desc ?? "Vui lòng thử lại."}",
                    code = paymentLink.Code,
                    desc = paymentLink.Desc,
                    error = "PayOs API returned error"
                });
            }

            _logger.LogInformation("[BACKEND] ✅ [CreateLink] Payment link created: PaymentLinkId={PaymentLinkId}", 
                paymentLink.Data.PaymentLinkId);
            
            // Log QR code details
            var hasQrCode = !string.IsNullOrEmpty(paymentLink.Data.QrCode);
            _logger.LogInformation("[BACKEND] 🔍 [CreateLink] QR Code in response: {HasQR}, Length: {Length}", 
                hasQrCode, paymentLink.Data.QrCode?.Length ?? 0);
            
            // Log account information để đảm bảo đúng tài khoản MB Bank
            _logger.LogInformation("[BACKEND] 🏦 [CreateLink] Account Number: {AccountNumber}, Account Name: {AccountName}", 
                paymentLink.Data.AccountNumber, paymentLink.Data.AccountName);
            
            // Validate account number - phải là 0901329227 (MB Bank)
            const string expectedAccountNumber = "0901329227";
            if (!string.IsNullOrEmpty(paymentLink.Data.AccountNumber) && 
                paymentLink.Data.AccountNumber != expectedAccountNumber)
            {
                _logger.LogWarning("[BACKEND] ⚠️ [CreateLink] Account Number mismatch! Expected: {Expected}, Got: {Actual}", 
                    expectedAccountNumber, paymentLink.Data.AccountNumber);
            }
            else if (paymentLink.Data.AccountNumber == expectedAccountNumber)
            {
                _logger.LogInformation("[BACKEND] ✅ [CreateLink] Account Number verified: {AccountNumber} (MB Bank)", 
                    paymentLink.Data.AccountNumber);
            }
            
            if (!hasQrCode)
            {
                _logger.LogWarning("[BACKEND] ⚠️ [CreateLink] PayOs did not return QR code. CheckoutUrl: {CheckoutUrl}", 
                    paymentLink.Data.CheckoutUrl);
            }

            return Ok(new
            {
                success = true,
                paymentLinkId = paymentLink.Data.PaymentLinkId,
                orderCode = paymentLink.Data.OrderCode,
                qrCode = paymentLink.Data.QrCode, // Base64 QR code image (may be null)
                checkoutUrl = paymentLink.Data.CheckoutUrl,
                amount = paymentLink.Data.Amount,
                description = paymentLink.Data.Description,
                accountNumber = paymentLink.Data.AccountNumber,
                accountName = paymentLink.Data.AccountName,
                expiredAt = paymentLink.Data.ExpiredAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BACKEND] ❌ [CreateLink] Exception creating payment link: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "[BACKEND] ❌ [CreateLink] Inner exception: {Message}", ex.InnerException.Message);
            }
            _logger.LogError("[BACKEND] ❌ [CreateLink] Stack trace: {StackTrace}", ex.StackTrace);
            return StatusCode(500, new { 
                message = "Lỗi tạo mã thanh toán", 
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// Endpoint để manually update booking status thành Paid (dùng khi webhook không hoạt động)
    /// </summary>
    [HttpPost("manual-update-paid/{bookingId}")]
    [Authorize(Roles = "Admin,FrontDesk,Manager")]
    public async Task<IActionResult> ManualUpdatePaid(int bookingId)
    {
        try
        {
            _logger.LogInformation("🔄 [ManualUpdate] Manually updating booking {BookingId} to Paid", bookingId);
            
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound(new { message = $"Booking {bookingId} không tồn tại" });
            }

            if (booking.Status == "Paid")
            {
                return Ok(new { message = "Booking đã được thanh toán rồi", bookingId, bookingCode = booking.BookingCode });
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Manual";
            var updated = await _bookingService.ProcessOnlinePaymentAsync(bookingId, userEmail);
            
            if (!updated)
            {
                return StatusCode(500, new { message = "Không thể cập nhật booking" });
            }

            _logger.LogInformation("✅ [ManualUpdate] Booking {BookingId} ({BookingCode}) updated to Paid", 
                bookingId, booking.BookingCode);

            return Ok(new 
            { 
                success = true,
                message = "Cập nhật thành công",
                bookingId,
                bookingCode = booking.BookingCode,
                status = "Paid"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ManualUpdate] Error updating booking {BookingId}", bookingId);
            return StatusCode(500, new { message = "Lỗi khi cập nhật booking", error = ex.Message });
        }
    }

    /// <summary>
    /// Tạo QR code động cho booking bằng SePay API
    /// </summary>
    [HttpPost("create-qr-booking")]
    [Authorize]
    public async Task<IActionResult> CreateBookingQRCode([FromBody] CreatePaymentLinkRequest request)
    {
        try
        {
            if (_sePayService == null)
            {
                _logger.LogWarning("[BACKEND] ⚠️ [CreateBookingQRCode] SePayService chưa được cấu hình");
                return BadRequest(new { message = "SePay service chưa được cấu hình. Vui lòng cấu hình SePay API credentials." });
            }

            _logger.LogInformation("[BACKEND] 🔄 [CreateBookingQRCode] Tạo QR code SePay cho booking {BookingId}", request.BookingId);

            // Get booking
            var booking = await _bookingService.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
            {
                return NotFound(new { message = $"Booking {request.BookingId} không tồn tại" });
            }

            // Check if already paid
            if (booking.Status == "Paid")
            {
                return BadRequest(new { message = "Đặt phòng này đã được thanh toán" });
            }

            // Get amount
            var amount = booking.EstimatedTotalAmount ?? 0;
            if (amount <= 0)
            {
                return BadRequest(new { message = "Số tiền thanh toán không hợp lệ" });
            }

            // Tạo đơn hàng và QR code qua SePay API
            // Duration: 24 giờ (86400 giây)
            var sepayOrder = await _sePayService.CreateBookingOrderAsync(request.BookingId, amount, durationSeconds: 86400);

            if (sepayOrder == null)
            {
                _logger.LogError("[BACKEND] ❌ [CreateBookingQRCode] SePay service returned null");
                return StatusCode(500, new { 
                    message = "Không thể tạo QR code. Vui lòng kiểm tra cấu hình SePay API hoặc thử lại sau.",
                    error = "SePay service returned null"
                });
            }

            _logger.LogInformation("[BACKEND] ✅ [CreateBookingQRCode] QR code tạo thành công: OrderId={OrderId}, OrderCode={OrderCode}", 
                sepayOrder.OrderId, sepayOrder.OrderCode);

            return Ok(new
            {
                success = true,
                orderId = sepayOrder.OrderId,
                orderCode = sepayOrder.OrderCode,
                qrCode = sepayOrder.QrCode, // Base64 image
                qrCodeUrl = sepayOrder.QrCodeUrl, // URL to QR code
                amount = sepayOrder.Amount,
                accountNumber = sepayOrder.AccountNumber,
                accountName = sepayOrder.AccountHolderName,
                bankName = sepayOrder.BankName,
                vaNumber = sepayOrder.VaNumber,
                expiredAt = sepayOrder.ExpiredAt,
                description = $"BOOKING{request.BookingId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BACKEND] ❌ [CreateBookingQRCode] Lỗi khi tạo QR code cho booking {BookingId}", request.BookingId);
            return StatusCode(500, new { 
                message = "Lỗi khi tạo QR code. Vui lòng thử lại.",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Tạo QR code động cho booking bằng VietQR (Miễn phí)
    /// </summary>
    [HttpPost("create-qr-booking-vietqr")]
    [Authorize]
    public async Task<IActionResult> CreateBookingQRCodeVietQR([FromBody] CreatePaymentLinkRequest request)
    {
        try
        {
            if (_vietQRService == null)
            {
                _logger.LogWarning("[BACKEND] ⚠️ [CreateBookingQRCodeVietQR] VietQRService chưa được cấu hình");
                return BadRequest(new { message = "VietQR service chưa được cấu hình. Vui lòng cấu hình bank account number." });
            }

            _logger.LogInformation("[BACKEND] 🔄 [CreateBookingQRCodeVietQR] Tạo QR code VietQR cho booking {BookingId}", request.BookingId);

            // Get booking
            var booking = await _bookingService.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
            {
                return NotFound(new { message = $"Booking {request.BookingId} không tồn tại" });
            }

            // Check if already paid
            if (booking.Status == "Paid")
            {
                return BadRequest(new { message = "Đặt phòng này đã được thanh toán" });
            }

            // Get amount
            var amount = booking.EstimatedTotalAmount ?? 0;
            if (amount <= 0)
            {
                return BadRequest(new { message = "Số tiền thanh toán không hợp lệ" });
            }

            // Tạo QR code URL bằng VietQR (miễn phí)
            var qrCodeUrl = _vietQRService.CreateBookingQRCode(request.BookingId, amount);

            if (string.IsNullOrEmpty(qrCodeUrl))
            {
                _logger.LogError("[BACKEND] ❌ [CreateBookingQRCodeVietQR] VietQR service returned null");
                return StatusCode(500, new { 
                    message = "Không thể tạo QR code. Vui lòng kiểm tra cấu hình bank account number.",
                    error = "VietQR service returned null"
                });
            }

            _logger.LogInformation("[BACKEND] ✅ [CreateBookingQRCodeVietQR] QR code tạo thành công: BookingId={BookingId}, Amount={Amount:N0} VND", 
                request.BookingId, amount);

            return Ok(new
            {
                success = true,
                orderId = $"BOOKING{request.BookingId}",
                orderCode = $"BOOKING{request.BookingId}",
                qrCode = (string?)null, // VietQR không có base64, chỉ có URL
                qrCodeUrl = qrCodeUrl, // URL to QR code image
                amount = (long)amount,
                accountNumber = _vietQRService.GetBankAccountNumber(),
                accountName = _vietQRService.GetBankAccountName(),
                bankName = _vietQRService.GetBankCode(),
                vaNumber = (string?)null,
                expiredAt = (string?)null,
                description = $"BOOKING{request.BookingId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BACKEND] ❌ [CreateBookingQRCodeVietQR] Lỗi khi tạo QR code cho booking {BookingId}", request.BookingId);
            return StatusCode(500, new { 
                message = "Lỗi khi tạo QR code. Vui lòng thử lại.",
                error = ex.Message
            });
        }
    }
}

