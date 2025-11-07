using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
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
            
            return CreatedAtAction(nameof(GetBookingById), new { id = createdBooking.BookingId }, bookingWithInvoice);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to create booking", error = ex.Message, stackTrace = ex.StackTrace });
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
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
            return NotFound(new { message = "Booking not found" });

        // Check authorization: customer can only view their own bookings
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var customerId = User.FindFirst("CustomerId")?.Value;

        if (userRole == "Customer" && customerId != booking.CustomerId.ToString())
            return Forbid();

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

