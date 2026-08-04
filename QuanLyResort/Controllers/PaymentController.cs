using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QuanLyResort.Hubs;
using QuanLyResort.Models;
using QuanLyResort.Services;
using System.Security.Claims;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentSessionService _paymentSessionService;
    private readonly IBookingService _bookingService;
    private readonly IBankWebhookService _bankWebhookService;
    private readonly IHubContext<PaymentHub> _hubContext;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentSessionService paymentSessionService,
        IBookingService bookingService,
        IBankWebhookService bankWebhookService,
        IHubContext<PaymentHub> hubContext,
        ILogger<PaymentController> logger)
    {
        _paymentSessionService = paymentSessionService;
        _bookingService = bookingService;
        _bankWebhookService = bankWebhookService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Tạo payment session cho booking
    /// </summary>
    [HttpPost("session/create")]
    public async Task<IActionResult> CreatePaymentSession([FromBody] CreatePaymentSessionRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            var customerId = int.TryParse(customerIdClaim ?? userIdClaim, out var id) ? id : 0;

            if (customerId <= 0)
            {
                return Unauthorized(new { message = "Không thể xác định khách hàng" });
            }

            // Lấy booking để verify ownership và amount
            var booking = await _bookingService.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            if (booking.CustomerId != customerId)
            {
                return Forbid("Bạn không có quyền thanh toán booking này");
            }

            if (booking.Status == "Paid")
            {
                return BadRequest(new { message = "Booking đã được thanh toán" });
            }

            var amount = request.Amount > 0 ? request.Amount : (booking.EstimatedTotalAmount ?? 0);
            if (amount <= 0)
            {
                return BadRequest(new { message = "Số tiền không hợp lệ" });
            }

            // Tạo payment session
            var session = await _paymentSessionService.CreateSessionAsync(
                request.BookingId, 
                customerId, 
                amount, 
                request.ExpiryMinutes ?? 15);

            _logger.LogInformation("Created payment session {SessionId} for booking {BookingId}", 
                session.SessionId, request.BookingId);

            return Ok(new
            {
                sessionId = session.SessionId,
                bookingId = session.BookingId,
                amount = session.Amount,
                expiresAt = session.ExpiresAt,
                status = session.Status.ToString().ToLower()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment session");
            return StatusCode(500, new { message = "Lỗi tạo payment session" });
        }
    }

    /// <summary>
    /// Lấy trạng thái payment session
    /// </summary>
    [HttpGet("status/{sessionId}")]
    public async Task<IActionResult> GetPaymentStatus(string sessionId)
    {
        try
        {
            var session = await _paymentSessionService.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound(new { message = "Session không tồn tại" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            var customerId = int.TryParse(customerIdClaim ?? userIdClaim, out var id) ? id : 0;

            if (session.CustomerId != customerId)
            {
                return Forbid("Bạn không có quyền xem session này");
            }

            var isExpired = await _paymentSessionService.IsSessionExpiredAsync(sessionId);
            
            return Ok(new
            {
                sessionId = session.SessionId,
                bookingId = session.BookingId,
                amount = session.Amount,
                status = session.Status.ToString().ToLower(),
                expiresAt = session.ExpiresAt,
                paidAt = session.PaidAt,
                transactionId = session.TransactionId,
                invoiceNumber = session.InvoiceNumber,
                errorMessage = session.ErrorMessage,
                isExpired = isExpired
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status");
            return StatusCode(500, new { message = "Lỗi lấy trạng thái payment" });
        }
    }

    /// <summary>
    /// Endpoint để test/simulate payment (chỉ dùng cho development/testing)
    /// </summary>
    [HttpPost("test/{bookingId}")]
    [Authorize]
    public async Task<IActionResult> TestPayment(int bookingId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            var customerId = int.TryParse(customerIdClaim ?? userIdClaim, out var id) ? id : 0;

            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            // Chỉ cho phép customer update booking của chính họ, hoặc admin
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim != "Admin" && booking.CustomerId != customerId)
            {
                return Forbid("Bạn không có quyền thực hiện thao tác này");
            }

            // Tìm payment session liên quan
            var sessions = await _paymentSessionService.GetSessionsByBookingIdAsync(bookingId);
            var activeSession = sessions?.FirstOrDefault(s => s.Status == PaymentStatus.Pending || s.Status == PaymentStatus.Processing);

            var transactionId = $"TEST-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var paidAt = DateTime.UtcNow;

            if (activeSession != null)
            {
                // Update session status
                await _paymentSessionService.UpdateSessionStatusAsync(
                    activeSession.SessionId,
                    PaymentStatus.Paid,
                    transactionId,
                    $"INV-{booking.BookingCode}",
                    null);

                // Broadcast qua SignalR cho payment session
                await _hubContext.Clients.Group($"payment_{activeSession.SessionId}").SendAsync("PaymentStatusChanged", new
                {
                    sessionId = activeSession.SessionId,
                    bookingId = bookingId,
                    status = "paid",
                    transactionId = transactionId,
                    invoiceNumber = $"INV-{booking.BookingCode}",
                    paidAt = paidAt,
                    errorMessage = (string?)null
                });
            }

            // Update booking status TRƯỚC khi broadcast
            var performedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "TestPayment";
            var paymentSuccess = await _bookingService.ProcessOnlinePaymentAsync(bookingId, performedBy);
            
            if (paymentSuccess)
            {
                // Broadcast cho booking group (fallback cho các client không có payment session)
                await _hubContext.Clients.Group($"booking_{bookingId}").SendAsync("BookingStatusChanged", new
                {
                    bookingId = bookingId,
                    status = "Paid",
                    transactionId = transactionId,
                    paidAt = paidAt
                });
            }

            if (!paymentSuccess)
            {
                return BadRequest(new { message = "Không thể cập nhật booking" });
            }

            _logger.LogInformation("Test payment processed for booking {BookingId}", bookingId);

            return Ok(new { 
                message = "Thanh toán test thành công",
                bookingId = bookingId,
                status = "Paid"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing test payment");
            return StatusCode(500, new { message = "Lỗi xử lý test payment" });
        }
    }

    /// <summary>
    /// Webhook endpoint cho VietQR
    /// </summary>
    [HttpPost("vietqr-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> VietQRWebhook([FromBody] VietQRWebhookDto request)
    {
        try
        {
            var vietQRService = HttpContext.RequestServices.GetRequiredService<VietQRWebhookService>();
            // Map Controller DTO to Service DTO
            var serviceDto = new Services.VietQRWebhookDto
            {
                TransactionId = request.TransactionId,
                VietQRTransactionId = request.VietQRTransactionId,
                Amount = request.Amount,
                Content = request.Content,
                AccountNumber = request.AccountNumber,
                AccountName = request.AccountName,
                BankCode = request.BankCode,
                BankName = request.BankName,
                TransactionDate = request.TransactionDate,
                Signature = request.Signature,
                Status = request.Status
            };
            var result = await vietQRService.ProcessVietQRWebhookAsync(serviceDto);

            if (!result.Success)
            {
                _logger.LogWarning("VietQR webhook processing failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                bookingId = result.BookingId,
                sessionId = result.PaymentSessionId,
                bookingUpdated = result.BookingUpdated
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VietQR webhook");
            return StatusCode(500, new { message = "Lỗi xử lý VietQR webhook" });
        }
    }

    /// <summary>
    /// Webhook endpoint cho PayOs (MB Bank Payment Gateway)
    /// </summary>
    [HttpPost("payos-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOsWebhook([FromBody] PayOsWebhookDto request)
    {
        try
        {
            var payOsService = HttpContext.RequestServices.GetRequiredService<PayOsWebhookService>();
            // Map Controller DTO to Service DTO
            var serviceDto = new Services.PayOsWebhookDto
            {
                Code = request.Code,
                Desc = request.Desc,
                Id = request.Id,
                Signature = request.Signature,
                Data = request.Data != null ? new Services.PayOsWebhookData
                {
                    TransactionId = request.Data.TransactionId,
                    Amount = request.Data.Amount,
                    Description = request.Data.Description,
                    AccountNumber = request.Data.AccountNumber,
                    AccountName = request.Data.AccountName,
                    TransactionDateTime = request.Data.TransactionDateTime,
                    Reference = request.Data.Reference,
                    Status = request.Data.Status
                } : null
            };
            var result = await payOsService.ProcessPayOsWebhookAsync(serviceDto);

            if (!result.Success)
            {
                _logger.LogWarning("PayOs webhook processing failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }

            _logger.LogInformation("PayOs webhook processed successfully: BookingId={BookingId}", result.BookingId);

            return Ok(new
            {
                message = result.Message,
                bookingId = result.BookingId,
                sessionId = result.PaymentSessionId,
                bookingUpdated = result.BookingUpdated
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayOs webhook");
            return StatusCode(500, new { message = "Lỗi xử lý PayOs webhook" });
        }
    }

    /// <summary>
    /// Webhook endpoint cho MB Bank
    /// </summary>
    [HttpPost("mbbank-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> MBBankWebhook([FromBody] MBBankWebhookDto request)
    {
        try
        {
            var mbBankService = HttpContext.RequestServices.GetRequiredService<MBBankWebhookService>();
            // Map Controller DTO to Service DTO
            var serviceDto = new Services.MBBankWebhookDto
            {
                TransactionId = request.TransactionId,
                MBTransactionId = request.MBTransactionId,
                Amount = request.Amount,
                Content = request.Content,
                TransactionDescription = request.TransactionDescription,
                AccountNumber = request.AccountNumber,
                AccountName = request.AccountName,
                ReferenceNumber = request.ReferenceNumber,
                TransactionDate = request.TransactionDate,
                Signature = request.Signature,
                Status = request.Status,
                TransactionType = request.TransactionType
            };
            var result = await mbBankService.ProcessMBBankWebhookAsync(serviceDto);

            if (!result.Success)
            {
                _logger.LogWarning("MB Bank webhook processing failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                bookingId = result.BookingId,
                sessionId = result.PaymentSessionId,
                bookingUpdated = result.BookingUpdated
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MB Bank webhook");
            return StatusCode(500, new { message = "Lỗi xử lý MB Bank webhook" });
        }
    }

    /// <summary>
    /// Webhook endpoint để nhận callback từ ngân hàng/API thanh toán (generic)
    /// </summary>
    [HttpPost("bank-webhook")]
    [AllowAnonymous] // Webhook từ ngân hàng không dùng JWT
    public async Task<IActionResult> BankWebhook([FromBody] BankWebhookRequestDto request)
    {
        try
        {
            var webhookRequest = new Services.BankWebhookRequest
            {
                BankName = request.BankName ?? "Unknown",
                TransactionId = request.TransactionId ?? string.Empty,
                Amount = request.Amount,
                Content = request.Content ?? string.Empty,
                AccountNumber = request.AccountNumber ?? string.Empty,
                AccountName = request.AccountName ?? string.Empty,
                TransactionDate = request.TransactionDate ?? DateTime.UtcNow,
                Signature = request.Signature,
                RawData = request.RawData
            };

            // Verify signature nếu có
            if (!string.IsNullOrEmpty(webhookRequest.Signature))
            {
                var isValid = await _bankWebhookService.VerifyWebhookSignatureAsync(webhookRequest, webhookRequest.Signature);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid webhook signature from {BankName}", webhookRequest.BankName);
                    return Unauthorized(new { message = "Invalid signature" });
                }
            }

            // Process webhook
            var result = await _bankWebhookService.ProcessWebhookAsync(webhookRequest);

            if (!result.Success)
            {
                _logger.LogWarning("Webhook processing failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                bookingId = result.BookingId,
                sessionId = result.PaymentSessionId,
                bookingUpdated = result.BookingUpdated
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bank webhook");
            return StatusCode(500, new { message = "Lỗi xử lý webhook" });
        }
    }

    /// <summary>
    /// Endpoint test để kiểm tra webhook có hoạt động không
    /// </summary>
    [HttpPost("test/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> TestWebhook([FromQuery] int bookingId = 39)
    {
        try
        {
            _logger.LogInformation("🧪 Test webhook endpoint called for booking {BookingId}", bookingId);

            // Simulate webhook request
            var testRequest = new BankWebhookRequest
            {
                BankName = "Test",
                TransactionId = $"TEST-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Amount = 15000,
                Content = $"BOOKING-{bookingId}",
                AccountNumber = "0901329227",
                AccountName = "Resort Deluxe",
                TransactionDate = DateTime.UtcNow,
                Signature = null,
                RawData = new Dictionary<string, object> { { "test", true } }
            };

            var result = await _bankWebhookService.ProcessWebhookAsync(testRequest);

            return Ok(new
            {
                success = result.Success,
                message = result.Message,
                bookingId = result.BookingId,
                sessionId = result.PaymentSessionId,
                bookingUpdated = result.BookingUpdated,
                test = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test webhook");
            return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Endpoint test để kiểm tra payment sessions và bookings trong DB
    /// </summary>
    [HttpGet("test/db-check")]
    [Authorize]
    public async Task<IActionResult> TestDBCheck([FromQuery] int? bookingId = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            var customerId = int.TryParse(customerIdClaim ?? userIdClaim, out var id) ? id : 0;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            var result = new
            {
                timestamp = DateTime.UtcNow,
                paymentSessions = new List<object>(),
                bookings = new List<object>(),
                invoices = new List<object>()
            };

            // 1. Check Payment Sessions (in-memory)
            var sessionsList = new List<object>();
            if (bookingId.HasValue)
            {
                var sessions = await _paymentSessionService.GetSessionsByBookingIdAsync(bookingId.Value);
                sessionsList = sessions.Select(s => new
                {
                    sessionId = s.SessionId,
                    bookingId = s.BookingId,
                    customerId = s.CustomerId,
                    amount = s.Amount,
                    status = s.Status.ToString(),
                    createdAt = s.CreatedAt,
                    expiresAt = s.ExpiresAt,
                    paidAt = s.PaidAt,
                    transactionId = s.TransactionId,
                    invoiceNumber = s.InvoiceNumber
                }).Cast<object>().ToList();
            }

            // 2. Check Bookings in DB
            Booking? booking = null;
            if (bookingId.HasValue)
            {
                booking = await _bookingService.GetBookingByIdAsync(bookingId.Value);
                
                // Check permissions
                if (roleClaim != "Admin" && booking?.CustomerId != customerId)
                {
                    return Forbid("Bạn không có quyền xem booking này");
                }
            }

            var bookingsList = new List<object>();
            if (booking != null)
            {
                bookingsList.Add(new
                {
                    bookingId = booking.BookingId,
                    bookingCode = booking.BookingCode,
                    customerId = booking.CustomerId,
                    status = booking.Status,
                    estimatedTotalAmount = booking.EstimatedTotalAmount,
                    checkInDate = booking.CheckInDate,
                    checkOutDate = booking.CheckOutDate,
                    createdAt = booking.CreatedAt,
                    updatedAt = booking.UpdatedAt,
                    invoice = booking.Invoice != null ? new
                    {
                        invoiceId = booking.Invoice.InvoiceId,
                        invoiceNumber = booking.Invoice.InvoiceNumber,
                        totalAmount = booking.Invoice.TotalAmount,
                        paidAmount = booking.Invoice.PaidAmount,
                        status = booking.Invoice.Status,
                        paidDate = booking.Invoice.PaidDate
                    } : (object?)null
                });
            }
            else if (roleClaim == "Admin")
            {
                // Admin có thể xem tất cả bookings với status "Paid"
                var allBookings = await _bookingService.GetAllBookingsAsync();
                var paidBookings = allBookings
                    .Where(b => b.Status == "Paid")
                    .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
                    .Take(10)
                    .ToList();
                
                // Load invoices for paid bookings
                foreach (var b in paidBookings)
                {
                    var bookingDetail = await _bookingService.GetBookingByIdAsync(b.BookingId);
                    bookingsList.Add(new
                    {
                        bookingId = bookingDetail?.BookingId ?? b.BookingId,
                        bookingCode = bookingDetail?.BookingCode ?? b.BookingCode,
                        customerId = bookingDetail?.CustomerId ?? b.CustomerId,
                        status = bookingDetail?.Status ?? b.Status,
                        estimatedTotalAmount = bookingDetail?.EstimatedTotalAmount ?? b.EstimatedTotalAmount,
                        updatedAt = bookingDetail?.UpdatedAt ?? b.UpdatedAt,
                        invoice = bookingDetail?.Invoice != null ? new
                        {
                            invoiceNumber = bookingDetail.Invoice.InvoiceNumber,
                            status = bookingDetail.Invoice.Status,
                            paidDate = bookingDetail.Invoice.PaidDate
                        } : null
                    });
                }
            }

            result = new { timestamp = result.timestamp, paymentSessions = sessionsList, bookings = bookingsList, invoices = result.invoices };

            return Ok(new
            {
                success = true,
                message = "Database check completed",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DB check");
            return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Webhook endpoint để nhận callback từ nhà cung cấp thanh toán (legacy)
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous] // Có thể cần xác thực bằng secret key
    public async Task<IActionResult> PaymentWebhook([FromBody] PaymentWebhookRequest request)
    {
        try
        {
            // TODO: Xác thực webhook bằng secret key/signature từ nhà cung cấp
            // Ví dụ: if (!VerifyWebhookSignature(request)) return Unauthorized();

            var session = await _paymentSessionService.GetSessionAsync(request.SessionId);
            if (session == null)
            {
                _logger.LogWarning("Webhook received for unknown session {SessionId}", request.SessionId);
                return NotFound(new { message = "Session không tồn tại" });
            }

            // Cập nhật trạng thái session
            PaymentStatus newStatus = request.Status.ToLower() switch
            {
                "paid" or "success" => PaymentStatus.Paid,
                "failed" or "error" => PaymentStatus.Failed,
                "cancelled" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Processing
            };

            bool updated = await _paymentSessionService.UpdateSessionStatusAsync(
                request.SessionId,
                newStatus,
                request.TransactionId,
                request.InvoiceNumber,
                request.ErrorMessage);

            if (!updated)
            {
                return BadRequest(new { message = "Không thể cập nhật session" });
            }

            // Nếu thanh toán thành công, cập nhật booking
            if (newStatus == PaymentStatus.Paid)
            {
                var performedBy = $"PaymentWebhook-{request.TransactionId}";
                var paymentSuccess = await _bookingService.ProcessOnlinePaymentAsync(session.BookingId, performedBy);
                
                if (!paymentSuccess)
                {
                    _logger.LogWarning("Failed to process payment for booking {BookingId}", session.BookingId);
                }
            }

            // Broadcast qua SignalR
            await _hubContext.Clients.Group($"payment_{request.SessionId}").SendAsync("PaymentStatusChanged", new
            {
                sessionId = request.SessionId,
                status = newStatus.ToString().ToLower(),
                transactionId = request.TransactionId,
                invoiceNumber = request.InvoiceNumber,
                paidAt = DateTime.UtcNow,
                errorMessage = request.ErrorMessage
            });

            _logger.LogInformation("Webhook processed for session {SessionId}, status: {Status}", 
                request.SessionId, newStatus);

            return Ok(new { message = "Webhook processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500, new { message = "Lỗi xử lý webhook" });
        }
    }
}

public class CreatePaymentSessionRequest
{
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public int? ExpiryMinutes { get; set; }
}

public class PaymentWebhookRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// DTO cho VietQR webhook
/// </summary>
public class VietQRWebhookDto
{
    public string? TransactionId { get; set; }
    public string? VietQRTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Content { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? BankCode { get; set; }
    public string? BankName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? Signature { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// DTO cho MB Bank webhook
/// </summary>
public class MBBankWebhookDto
{
    public string? TransactionId { get; set; }
    public string? MBTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Content { get; set; }
    public string? TransactionDescription { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? Signature { get; set; }
    public string? Status { get; set; }
    public string? TransactionType { get; set; }
}

/// <summary>
/// DTO cho PayOs webhook
/// </summary>
public class PayOsWebhookDto
{
    public int Code { get; set; } // 0 = thành công
    public string? Desc { get; set; }
    public PayOsWebhookDataDto? Data { get; set; }
    public string? Signature { get; set; }
    public string? Id { get; set; }
}

public class PayOsWebhookDataDto
{
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public DateTime? TransactionDateTime { get; set; }
    public string? Reference { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// DTO cho bank webhook request
/// </summary>
public class BankWebhookRequestDto
{
    public string? BankName { get; set; } // "MB", "VCB", "TCB", "VietQR", etc.
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Content { get; set; } // Nội dung chuyển khoản
    public string? AccountNumber { get; set; } // Số tài khoản nhận
    public string? AccountName { get; set; } // Tên tài khoản nhận
    public DateTime? TransactionDate { get; set; }
    public string? Signature { get; set; } // Signature để verify
    public Dictionary<string, object>? RawData { get; set; } // Dữ liệu raw từ ngân hàng
}

