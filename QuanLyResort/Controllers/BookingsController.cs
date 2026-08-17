using QuanLyResort.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Services;
using System.Security.Claims;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookingsController> _logger;
    private readonly INotificationManager _notificationManager;

    public BookingsController(
        IBookingService bookingService, 
        IUnitOfWork unitOfWork, 
        ILogger<BookingsController> logger,
        INotificationManager notificationManager)
    {
        _bookingService = bookingService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _notificationManager = notificationManager;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";
            
            // Validate request
            if (request.CustomerId <= 0)
            {
                return BadRequest(new { message = "CustomerId is required and must be valid" });
            }

            // Validate CustomerId exists in database
            var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == request.CustomerId);
            if (!customerExists)
            {
                Console.WriteLine($"❌ [CreateBooking] CustomerId {request.CustomerId} does not exist in database");
                return BadRequest(new { message = $"CustomerId {request.CustomerId} không tồn tại trong hệ thống" });
            }

            if (request.CheckOutDate <= request.CheckInDate)
            {
                return BadRequest(new { message = "Check-out date must be after check-in date" });
            }

            var booking = new Booking
            {
                CustomerId = request.CustomerId,
                RequestedRoomType = request.RequestedRoomType,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                NumberOfGuests = request.NumberOfGuests,
                SpecialRequests = request.SpecialRequests,
                Source = request.Source ?? "Direct"
            };

            var createdBooking = await _bookingService.CreateBookingAsync(booking, userEmail);
            
            // Ensure invoice is loaded for response
            var bookingWithInvoice = await _bookingService.GetBookingByIdAsync(createdBooking.BookingId);
            
            // Send notification (email, SMS, in-app)
            if (bookingWithInvoice != null && bookingWithInvoice.Invoice != null)
            {
                var bookingCode = $"BK{createdBooking.BookingId:D6}";
                var totalAmount = bookingWithInvoice.Invoice.TotalAmount;
                _ = Task.Run(async () =>
                {
                    await _notificationManager.SendBookingConfirmationAsync(
                        request.CustomerId,
                        bookingCode,
                        request.CheckInDate,
                        request.CheckOutDate,
                        totalAmount
                    );
                });
            }
            
            return CreatedAtAction(nameof(GetBookingById), new { id = createdBooking.BookingId }, bookingWithInvoice);
        }
        catch (Exception ex)
        {
            // Log chi tiết lỗi để debug
            Console.WriteLine($"❌ [CreateBooking] Error: {ex.Message}");
            Console.WriteLine($"❌ [CreateBooking] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"❌ [CreateBooking] InnerException: {ex.InnerException.Message}");
            }
            
            return StatusCode(500, new { 
                message = "Failed to create booking", 
                error = ex.Message,
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace 
            });
        }
    }

    [HttpGet("my")]
    [Authorize(Roles = "Customer,Admin,FrontDesk,Manager")]
    public async Task<IActionResult> GetMyBookings()
    {
        // Lấy CustomerId từ JWT claims
        var userCustomerId = User.FindFirst("CustomerId")?.Value;
        if (string.IsNullOrWhiteSpace(userCustomerId) || !int.TryParse(userCustomerId, out var customerId))
        {
            return Forbid();
        }

        var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId);
        return Ok(bookings);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,FrontDesk,Manager,Cashier")]
    public async Task<IActionResult> GetAllBookings()
    {
        var bookings = await _bookingService.GetAllBookingsAsync();
        return Ok(bookings);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBookingById(int id)
    {
        _logger.LogInformation($"[GetBookingById] 📥 Request to get booking {id}");
        
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
        {
            _logger.LogWarning($"[GetBookingById] ❌ Booking {id} not found");
            return NotFound(new { message = "Booking not found" });
        }

        // Check authorization: customer can only view their own bookings
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var customerId = User.FindFirst("CustomerId")?.Value;

        if (userRole == "Customer" && customerId != booking.CustomerId.ToString())
        {
            _logger.LogWarning($"[GetBookingById] 🚫 Forbidden: Customer {customerId} trying to access booking {id} (belongs to {booking.CustomerId})");
            return Forbid();
        }
        
        _logger.LogInformation($"[GetBookingById] ✅ Returning booking {id} - Status: '{booking.Status}', CustomerId: {booking.CustomerId}, BookingCode: '{booking.BookingCode}'");
        return Ok(booking);
    }

    [HttpGet("customer/{customerId:int}")]
    [Authorize(Roles = "Customer,Admin,FrontDesk,Manager")]
    public async Task<IActionResult> GetBookingsByCustomer(int customerId)
    {
        // Customer can only view their own bookings
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userCustomerId = User.FindFirst("CustomerId")?.Value;

        if (userRole == "Customer" && userCustomerId != customerId.ToString())
            return Forbid();

        var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId);
        return Ok(bookings);
    }

    [HttpPost("{id}/transfer-to-frontdesk")]
    [Authorize(Roles = "Customer,Admin,FrontDesk")]
    public async Task<IActionResult> TransferToFrontDesk(int id)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";
        var success = await _bookingService.TransferToFrontDeskAsync(id, userEmail);

        if (!success)
            return BadRequest(new { message = "Unable to transfer booking" });

        return Ok(new { message = "Booking transferred to front desk successfully" });
    }

    [HttpPost("{id}/assign-room")]
    [Authorize(Roles = "Admin,FrontDesk,Manager")]
    public async Task<IActionResult> AssignRoom(int id, [FromBody] AssignRoomRequest request)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";
        var success = await _bookingService.AssignRoomAsync(id, request.RoomId, userEmail);

        if (!success)
            return BadRequest(new { message = "Unable to assign room. Room may not be available or booking may have overlapping dates." });

        return Ok(new { message = "Room assigned successfully" });
    }

    [HttpPost("{id}/checkin")]
    [Authorize(Roles = "Admin,FrontDesk")]
    public async Task<IActionResult> CheckIn(int id)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";
        var success = await _bookingService.CheckInAsync(id, userEmail);

        if (!success)
            return BadRequest(new { message = "Unable to check in. Booking may not be in Assigned status." });

        return Ok(new { message = "Check-in successful" });
    }

    [HttpPost("{id}/add-charge")]
    [Authorize(Roles = "Admin,FrontDesk,Cashier")]
    public async Task<IActionResult> AddCharge(int id, [FromBody] Charge charge)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";
        var success = await _bookingService.AddChargeAsync(id, charge, userEmail);

        if (!success)
            return BadRequest(new { message = "Unable to add charge. Booking may not be checked in." });

        return Ok(new { message = "Charge added successfully" });
    }

    [HttpPost("{id}/checkout")]
    [Authorize(Roles = "Admin,FrontDesk,Cashier")]
    public async Task<IActionResult> CheckOut(int id)
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";
            var invoice = await _bookingService.CheckOutAsync(id, userEmail);
            return Ok(new { message = "Check-out successful", invoice });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingRequest request)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";
        var success = await _bookingService.CancelBookingAsync(id, request.Reason, userEmail);

        if (!success)
            return BadRequest(new { message = "Unable to cancel booking" });

        return Ok(new { message = "Booking cancelled successfully" });
    }

    /// <summary>
    /// User yêu cầu thanh toán tiền mặt (chờ admin xác nhận)
    /// </summary>
    [HttpPost("{id}/request-cash-payment")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> RequestCashPayment(int id)
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";
            
            _logger.LogInformation($"[RequestCashPayment] 🔄 Customer {userEmail} (Role: {userRole}) requesting cash payment for booking {id}");
            
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                _logger.LogWarning($"[RequestCashPayment] ❌ Booking {id} not found");
                return NotFound(new { message = "Không tìm thấy đặt phòng" });
            }
            
            _logger.LogInformation($"[RequestCashPayment] 📋 Booking {id} current status: Status='{booking.Status}', BookingCode='{booking.BookingCode}', CustomerId={booking.CustomerId}");
            
            // Kiểm tra authorization: customer chỉ có thể request cho booking của mình
            var customerId = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerId) || !int.TryParse(customerId, out int userCustomerId) || booking.CustomerId != userCustomerId)
            {
                return Forbid();
            }
            
            if (booking.Status == "Paid")
            {
                return BadRequest(new { message = "Đặt phòng này đã được thanh toán rồi" });
            }
            
            if (booking.Status != "Pending" && booking.Status != "Confirmed")
            {
                return BadRequest(new { message = $"Không thể yêu cầu thanh toán khi đặt phòng đang ở trạng thái '{booking.Status}'" });
            }
            
            // Lưu thông tin yêu cầu thanh toán tiền mặt vào SpecialRequests
            var specialRequests = booking.SpecialRequests;
            Dictionary<string, object>? requestsDict = null;
            
            try
            {
                if (!string.IsNullOrEmpty(specialRequests))
                {
                    requestsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(specialRequests);
                }
            }
            catch { }
            
            if (requestsDict == null)
            {
                requestsDict = new Dictionary<string, object>();
            }
            
            requestsDict["cashPaymentRequested"] = true;
            requestsDict["cashPaymentRequestedAt"] = DateTime.UtcNow.ToString("O");
            requestsDict["cashPaymentRequestedBy"] = userEmail;
            
            booking.SpecialRequests = System.Text.Json.JsonSerializer.Serialize(requestsDict);
            booking.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation($"[RequestCashPayment] ✅✅✅ SUCCESS: Cash payment request saved for booking {id}. Status='{booking.Status}', SpecialRequests updated");
            
            return Ok(new { 
                message = "Yêu cầu thanh toán tiền mặt đã được gửi. Vui lòng chờ admin xác nhận.", 
                bookingId = id,
                status = booking.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[RequestCashPayment] ❌ Exception requesting cash payment for booking {id}");
            return StatusCode(500, new { message = "Lỗi khi xử lý yêu cầu thanh toán", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Admin xác nhận thanh toán tiền mặt
    /// </summary>
    [HttpPost("{id}/approve-cash-payment")]
    [Authorize(Roles = "Admin,FrontDesk,Cashier")]
    public async Task<IActionResult> ApproveCashPayment(int id)
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";
            
            _logger.LogInformation($"[ApproveCashPayment] 🔄 Admin {userEmail} (Role: {userRole}) approving cash payment for booking {id}");
            
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                _logger.LogWarning($"[ApproveCashPayment] ❌ Booking {id} not found");
                return NotFound(new { message = "Không tìm thấy đặt phòng" });
            }
            
            _logger.LogInformation($"[ApproveCashPayment] 📋 Booking {id} current status: Status='{booking.Status}', BookingCode='{booking.BookingCode}', CustomerId={booking.CustomerId}");
            
            if (booking.Status == "Paid")
            {
                _logger.LogWarning($"[ApproveCashPayment] ⚠️ Booking {id} already paid");
                return BadRequest(new { message = "Đặt phòng này đã được thanh toán rồi" });
            }
            
            // Kiểm tra xem có yêu cầu thanh toán tiền mặt không
            var hasCashPaymentRequest = false;
            if (!string.IsNullOrEmpty(booking.SpecialRequests))
            {
                try
                {
                    var requestsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(booking.SpecialRequests);
                    if (requestsDict != null && requestsDict.ContainsKey("cashPaymentRequested"))
                    {
                        hasCashPaymentRequest = true;
                        _logger.LogInformation($"[ApproveCashPayment] ✅ Found cash payment request in SpecialRequests");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"[ApproveCashPayment] ⚠️ Error parsing SpecialRequests: {ex.Message}");
                }
            }
            
            if (!hasCashPaymentRequest)
            {
                _logger.LogWarning($"[ApproveCashPayment] ❌ No cash payment request found for booking {id}");
                return BadRequest(new { message = "Không có yêu cầu thanh toán tiền mặt cho đặt phòng này" });
            }
            
            _logger.LogInformation($"[ApproveCashPayment] 💰 Processing payment for booking {id}...");
            
            // Xử lý thanh toán (giống như ProcessOnlinePaymentAsync)
            var success = await _bookingService.ProcessOnlinePaymentAsync(id, userEmail);
            
            if (!success)
            {
                _logger.LogError($"[ApproveCashPayment] ❌ Failed to process payment for booking {id}");
                return BadRequest(new { message = "Không thể xử lý thanh toán. Vui lòng thử lại sau hoặc liên hệ hỗ trợ." });
            }
            
            _logger.LogInformation($"[ApproveCashPayment] ✅ Payment processed successfully for booking {id}");
            
            // Xóa thông tin yêu cầu thanh toán tiền mặt khỏi SpecialRequests
            var specialRequests = booking.SpecialRequests;
            if (!string.IsNullOrEmpty(specialRequests))
            {
                try
                {
                    var requestsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(specialRequests);
                    if (requestsDict != null)
                    {
                        requestsDict.Remove("cashPaymentRequested");
                        requestsDict.Remove("cashPaymentRequestedAt");
                        requestsDict.Remove("cashPaymentRequestedBy");
                        requestsDict["cashPaymentApproved"] = true;
                        requestsDict["cashPaymentApprovedAt"] = DateTime.UtcNow.ToString("O");
                        requestsDict["cashPaymentApprovedBy"] = userEmail;
                        
                        var updatedBooking = await _bookingService.GetBookingByIdAsync(id);
                        if (updatedBooking != null)
                        {
                            updatedBooking.SpecialRequests = System.Text.Json.JsonSerializer.Serialize(requestsDict);
                            updatedBooking.UpdatedAt = DateTime.UtcNow;
                            await _unitOfWork.SaveChangesAsync();
                            _logger.LogInformation($"[ApproveCashPayment] ✅ Updated SpecialRequests for booking {id}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"[ApproveCashPayment] ⚠️ Error updating SpecialRequests: {ex.Message}");
                }
            }
            
            var updatedBookingFinal = await _bookingService.GetBookingByIdAsync(id);
            var invoiceNumber = updatedBookingFinal?.Invoice?.InvoiceNumber;
            
            _logger.LogInformation($"[ApproveCashPayment] ✅✅✅ SUCCESS: Booking {id} approved! Final Status='{updatedBookingFinal?.Status}', InvoiceNumber='{invoiceNumber}'");
            
            return Ok(new { 
                message = "Xác nhận thanh toán tiền mặt thành công", 
                bookingId = id, 
                paid = true,
                invoiceNumber = invoiceNumber,
                status = updatedBookingFinal?.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[ApproveCashPayment] ❌ Exception approving cash payment for booking {id}");
            return StatusCode(500, new { message = "Lỗi khi xác nhận thanh toán", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Xử lý thanh toán online cho booking
    /// </summary>
    [HttpPost("{id}/pay-online")]
    [Authorize(Roles = "Customer,Admin,FrontDesk,Cashier")]
    public async Task<IActionResult> PayOnline(int id)
    {
        try
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "system";
            
            // Lấy thông tin booking để kiểm tra
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy đặt phòng" });
            }
            
            // Kiểm tra trạng thái booking trước khi thanh toán
            if (booking.Status == "Paid")
            {
                return BadRequest(new { message = "Đặt phòng này đã được thanh toán rồi" });
            }
            
            if (booking.Status != "Pending" && booking.Status != "Confirmed")
            {
                return BadRequest(new { message = $"Không thể thanh toán khi đặt phòng đang ở trạng thái '{booking.Status}'. Chỉ có thể thanh toán khi đặt phòng đang chờ xác nhận hoặc đã được xác nhận." });
            }
            
            var success = await _bookingService.ProcessOnlinePaymentAsync(id, userEmail);
            
            if (!success)
            {
                return BadRequest(new { message = "Không thể xử lý thanh toán. Vui lòng thử lại sau hoặc liên hệ hỗ trợ." });
            }

            // Lấy lại booking để lấy thông tin invoice mới tạo
            var updatedBooking = await _bookingService.GetBookingByIdAsync(id);
            var invoiceNumber = updatedBooking?.Invoice?.InvoiceNumber;

            return Ok(new { 
                message = "Thanh toán thành công", 
                bookingId = id, 
                paid = true,
                invoiceNumber = invoiceNumber,
                status = updatedBooking?.Status
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xử lý thanh toán", error = ex.Message });
        }
    }

    [HttpGet("room/{roomId:int}/booked-dates")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBookedDatesForRoom(int roomId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            
            // Tìm các booking được Confirm hoặc CheckIn cho phòng này, từ hôm nay trở đi
            var bookings = await _context.Bookings
                .Where(b => b.RoomId == roomId 
                         && (b.Status == "Confirmed" || b.Status == "CheckedIn")
                         && b.CheckOutDate >= today)
                .Select(b => new { b.CheckInDate, b.CheckOutDate })
                .ToListAsync();

            var bookedDates = new List<string>();

            foreach (var b in bookings)
            {
                var current = b.CheckInDate.Date;
                var end = b.CheckOutDate.Date;
                
                // Khách check-out vào một ngày, ngày đó coi như vẫn có thể cho khách mới check-in (tùy nghiệp vụ resort)
                // Tuy nhiên Flatpickr disable mảng ngày, ta disable hết các ngày từ CheckIn đến (CheckOut - 1 day)
                while (current < end)
                {
                    bookedDates.Add(current.ToString("yyyy-MM-dd"));
                    current = current.AddDays(1);
                }
            }

            return Ok(bookedDates.Distinct().OrderBy(d => d).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booked dates for room {RoomId}", roomId);
            return StatusCode(500, new { message = "Failed to get booked dates", error = ex.Message });
        }
    }
}

public class CreateBookingRequest
{
    public int CustomerId { get; set; }
    public string RequestedRoomType { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public string? SpecialRequests { get; set; }
    public string? Source { get; set; }
}

public class AssignRoomRequest
{
    public int RoomId { get; set; }
}

public class CancelBookingRequest
{
    public string Reason { get; set; } = string.Empty;
}




