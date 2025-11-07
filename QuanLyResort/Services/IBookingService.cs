using QuanLyResort.Models;

namespace QuanLyResort.Services;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Booking booking, string createdBy);
    Task<Booking?> GetBookingByIdAsync(int bookingId);
    Task<Booking?> GetBookingByCodeAsync(string bookingCode);
    Task<IEnumerable<Booking>> GetAllBookingsAsync();
    Task<IEnumerable<Booking>> GetBookingsByCustomerAsync(int customerId);
    Task<bool> TransferToFrontDeskAsync(int bookingId, string performedBy);
    Task<bool> AssignRoomAsync(int bookingId, int roomId, string performedBy);
    Task<bool> CheckInAsync(int bookingId, string performedBy);
    Task<bool> AddChargeAsync(int bookingId, Charge charge, string createdBy);
    Task<Invoice> CheckOutAsync(int bookingId, string performedBy);
    Task<bool> CancelBookingAsync(int bookingId, string reason, string performedBy);
    Task<bool> ProcessOnlinePaymentAsync(int bookingId, string performedBy);
}

