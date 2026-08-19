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
/// Controller don gi?n cho thanh toán - t?o PayOs payment link và x? lý webhook
/// </summary>
[ApiController]
    [Route("api/simplepayment")]
    public class RestaurantPaymentController : ControllerBase
    {
    private readonly IBookingService _bookingService;
    private readonly PayOsService _payOsService;
    private readonly SePayService? _sePayService;
    private readonly VietQRService? _vietQRService;
    private readonly ILogger<RestaurantPaymentController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private ResortDbContext _context => _unitOfWork.Context;

    public RestaurantPaymentController(
        IBookingService bookingService,
        PayOsService payOsService,
        SePayService? sePayService,
        VietQRService? vietQRService,
        ILogger<RestaurantPaymentController> logger,
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
    /// T?o PayOs payment link cho restaurant order
    /// </summary>
    [HttpPost("create-link-restaurant")]
    [Authorize]
    public async Task<IActionResult> CreateRestaurantPaymentLink([FromBody] CreateRestaurantPaymentLinkRequest request)
    {
        try
        {
            _logger.LogInformation("[BACKEND] ?? [CreateRestaurantLink] Creating PayOs payment link for restaurant order {OrderId}", request.OrderId);

            // Get restaurant order
            var order = await _context.RestaurantOrders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);
            
            if (order == null)
            {
                return NotFound(new { message = $"Restaurant order {request.OrderId} không t?n t?i" });
            }

            // Check authorization - customer ch? có th? thanh toán don c?a mình
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            
            if (userRole == "Customer")
            {
                if (order.CustomerId == null)
                {
                    return BadRequest(new { message = "Ðon hàng này là don t?i qu?y, vui lòng thanh toán tr?c ti?p t?i nhà hàng" });
                }
                
                if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId) || order.CustomerId != customerId)
                {
                    return StatusCode(403, new { message = "B?n ch? có th? thanh toán don hàng c?a chính b?n" });
                }
            }

            // Check if already paid
            if (order.PaymentStatus == "Paid")
            {
                return BadRequest(new { message = "Ðon hàng này dã du?c thanh toán" });
            }

            // Get amount
            var amount = order.TotalAmount;
            if (amount <= 0)
            {
                return BadRequest(new { message = "S? ti?n thanh toán không h?p l?" });
            }

            // T?o orderCode unique - dùng format khác v?i booking d? tránh conflict
            // Restaurant order: orderCode = 20000000 + orderId * 10000 + timestamp
            var timestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            var orderCode = 20000000L + request.OrderId * 10000L + (timestamp % 10000);
            var description = $"ORDER{request.OrderId}"; // PayOs description
            
            // Get base URL from request
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var returnUrl = $"{baseUrl}/customer/order-details.html?orderId={request.OrderId}&payment=success";
            var cancelUrl = $"{baseUrl}/customer/order-details.html?orderId={request.OrderId}&payment=cancelled";

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
                _logger.LogError("[BACKEND] ? [CreateRestaurantLink] PayOs service returned null");
                return StatusCode(500, new { 
                    message = "Không th? t?o mã thanh toán. Vui lòng th? l?i.",
                    error = "PayOs service returned null"
                });
            }

            if (paymentLink.Data == null)
            {
                // N?u l?i "Ðon thanh toán dã t?n t?i", th? l?y payment link hi?n có
                if (paymentLink.Desc?.Contains("dã t?n t?i") == true || 
                    paymentLink.Desc?.Contains("already exists") == true ||
                    paymentLink.Code == "03")
                {
                    _logger.LogWarning("[BACKEND] ?? [CreateRestaurantLink] Payment link already exists for orderCode {OrderCode}. Trying to get existing link...", orderCode);
                    
                    var existingLink = await _payOsService.GetPaymentLinkByOrderCodeAsync(orderCode);
                    if (existingLink?.Data != null)
                    {
                        _logger.LogInformation("[BACKEND] ? [CreateRestaurantLink] Found existing payment link: PaymentLinkId={PaymentLinkId}", 
                            existingLink.Data.PaymentLinkId);
                        
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
                
                _logger.LogError("[BACKEND] ? [CreateRestaurantLink] PayOs returned error. Code: {Code}, Desc: {Desc}", 
                    paymentLink.Code, paymentLink.Desc);
                return StatusCode(500, new { 
                    message = $"Không th? t?o mã thanh toán. {paymentLink.Desc ?? "Vui lòng th? l?i."}",
                    code = paymentLink.Code,
                    desc = paymentLink.Desc,
                    error = "PayOs API returned error"
                });
            }

            _logger.LogInformation("[BACKEND] ? [CreateRestaurantLink] Payment link created: PaymentLinkId={PaymentLinkId}", 
                paymentLink.Data.PaymentLinkId);
            
            // Log QR code details
            var hasQrCode = !string.IsNullOrEmpty(paymentLink.Data.QrCode);
            _logger.LogInformation("[BACKEND] ?? [CreateRestaurantLink] QR Code in response: {HasQR}, Length: {Length}", 
                hasQrCode, paymentLink.Data.QrCode?.Length ?? 0);
            
            // Log account information
            _logger.LogInformation("[BACKEND] ?? [CreateRestaurantLink] Account Number: {AccountNumber}, Account Name: {AccountName}", 
                paymentLink.Data.AccountNumber, paymentLink.Data.AccountName);
            
            // Validate account number - ph?i là 0901329227 (MB Bank)
            const string expectedAccountNumber = "0901329227";
            if (!string.IsNullOrEmpty(paymentLink.Data.AccountNumber) && 
                paymentLink.Data.AccountNumber != expectedAccountNumber)
            {
                _logger.LogWarning("[BACKEND] ?? [CreateRestaurantLink] Account Number mismatch! Expected: {Expected}, Got: {Actual}", 
                    expectedAccountNumber, paymentLink.Data.AccountNumber);
            }
            else if (paymentLink.Data.AccountNumber == expectedAccountNumber)
            {
                _logger.LogInformation("[BACKEND] ? [CreateRestaurantLink] Account Number verified: {AccountNumber} (MB Bank)", 
                    paymentLink.Data.AccountNumber);
            }
            
            if (!hasQrCode)
            {
                _logger.LogWarning("[BACKEND] ?? [CreateRestaurantLink] PayOs did not return QR code. CheckoutUrl: {CheckoutUrl}", 
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
            _logger.LogError(ex, "[BACKEND] ? [CreateRestaurantLink] Exception creating payment link: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "[BACKEND] ? [CreateRestaurantLink] Inner exception: {Message}", ex.InnerException.Message);
            }
            _logger.LogError("[BACKEND] ? [CreateRestaurantLink] Stack trace: {StackTrace}", ex.StackTrace);
            return StatusCode(500, new { 
                message = "L?i t?o mã thanh toán", 
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// T?o QR code d?ng cho restaurant order b?ng SePay API
    /// </summary>
    [HttpPost("create-qr-restaurant")]
    [Authorize]
    public async Task<IActionResult> CreateRestaurantQRCode([FromBody] CreateRestaurantPaymentLinkRequest request)
    {
        try
        {
            if (_sePayService == null)
            {
                _logger.LogWarning("[BACKEND] ?? [CreateRestaurantQRCode] SePayService chua du?c c?u hình");
                return BadRequest(new { message = "SePay service chua du?c c?u hình. Vui lòng c?u hình SePay API credentials." });
            }

            _logger.LogInformation("[BACKEND] ?? [CreateRestaurantQRCode] T?o QR code SePay cho restaurant order {OrderId}", request.OrderId);

            // Get restaurant order
            var order = await _context.RestaurantOrders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);

            if (order == null)
            {
                return NotFound(new { message = $"Restaurant order {request.OrderId} không t?n t?i" });
            }

            // Check if already paid
            if (order.Status == "Paid" || order.Status == "Completed")
            {
                return BadRequest(new { message = "Ðon hàng này dã du?c thanh toán" });
            }

            // Get amount
            var amount = order.TotalAmount;
            if (amount <= 0)
            {
                return BadRequest(new { message = "S? ti?n thanh toán không h?p l?" });
            }

            // T?o don hàng và QR code qua SePay API
            // Duration: 24 gi? (86400 giây)
            var sepayOrder = await _sePayService.CreateRestaurantOrderAsync(request.OrderId, amount, durationSeconds: 86400);

            if (sepayOrder == null)
            {
                _logger.LogError("[BACKEND] ? [CreateRestaurantQRCode] SePay service returned null");
                return StatusCode(500, new { 
                    message = "Không th? t?o QR code. Vui lòng ki?m tra c?u hình SePay API ho?c th? l?i sau.",
                    error = "SePay service returned null"
                });
            }

            _logger.LogInformation("[BACKEND] ? [CreateRestaurantQRCode] QR code t?o thành công: OrderId={OrderId}, OrderCode={OrderCode}", 
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
                description = $"ORDER{request.OrderId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BACKEND] ? [CreateRestaurantQRCode] L?i khi t?o QR code cho restaurant order {OrderId}", request.OrderId);
            return StatusCode(500, new { 
                message = "L?i khi t?o QR code. Vui lòng th? l?i.",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// T?o QR code d?ng cho restaurant order b?ng VietQR (Mi?n phí)
    /// </summary>
    [HttpPost("create-qr-restaurant-vietqr")]
    [Authorize]
    public async Task<IActionResult> CreateRestaurantQRCodeVietQR([FromBody] CreateRestaurantPaymentLinkRequest request)
    {
        try
        {
            if (_vietQRService == null)
            {
                _logger.LogWarning("[BACKEND] ?? [CreateRestaurantQRCodeVietQR] VietQRService chua du?c c?u hình");
                return BadRequest(new { message = "VietQR service chua du?c c?u hình. Vui lòng c?u hình bank account number." });
            }

            _logger.LogInformation("[BACKEND] ?? [CreateRestaurantQRCodeVietQR] T?o QR code VietQR cho restaurant order {OrderId}", request.OrderId);

            // Get restaurant order
            var order = await _context.RestaurantOrders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);

            if (order == null)
            {
                return NotFound(new { message = $"Restaurant order {request.OrderId} không t?n t?i" });
            }

            // Check if already paid
            if (order.Status == "Paid" || order.Status == "Completed")
            {
                return BadRequest(new { message = "Ðon hàng này dã du?c thanh toán" });
            }

            // Get amount
            var amount = order.TotalAmount;
            if (amount <= 0)
            {
                return BadRequest(new { message = "S? ti?n thanh toán không h?p l?" });
            }

            // T?o QR code URL b?ng VietQR (mi?n phí)
            var qrCodeUrl = _vietQRService.CreateRestaurantOrderQRCode(request.OrderId, amount);

            if (string.IsNullOrEmpty(qrCodeUrl))
            {
                _logger.LogError("[BACKEND] ? [CreateRestaurantQRCodeVietQR] VietQR service returned null");
                return StatusCode(500, new { 
                    message = "Không th? t?o QR code. Vui lòng ki?m tra c?u hình bank account number.",
                    error = "VietQR service returned null"
                });
            }

            _logger.LogInformation("[BACKEND] ? [CreateRestaurantQRCodeVietQR] QR code t?o thành công: OrderId={OrderId}, Amount={Amount:N0} VND", 
                request.OrderId, amount);

            return Ok(new
            {
                success = true,
                orderId = $"ORDER{request.OrderId}",
                orderCode = $"ORDER{request.OrderId}",
                qrCode = (string?)null, // VietQR không có base64, ch? có URL
                qrCodeUrl = qrCodeUrl, // URL to QR code image
                amount = (long)amount,
                accountNumber = _vietQRService.GetBankAccountNumber(),
                accountName = _vietQRService.GetBankAccountName(),
                bankName = _vietQRService.GetBankCode(),
                vaNumber = (string?)null,
                expiredAt = (string?)null,
                description = $"ORDER{request.OrderId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BACKEND] ? [CreateRestaurantQRCodeVietQR] L?i khi t?o QR code cho restaurant order {OrderId}", request.OrderId);
            return StatusCode(500, new { 
                message = "L?i khi t?o QR code. Vui lòng th? l?i.",
                error = ex.Message
            });
        }
    }
}

