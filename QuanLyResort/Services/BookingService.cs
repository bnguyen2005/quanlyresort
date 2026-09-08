using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Repositories;

namespace QuanLyResort.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private ResortDbContext _context => _unitOfWork.Context;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
private readonly ILogger<BookingService> _logger;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public BookingService(IUnitOfWork unitOfWork, IAuditService auditService, INotificationService notificationService, ILogger<BookingService> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _auditService = auditService;
        _notificationService = notificationService;
        _cache = cache;
}

    public async Task<Booking> CreateBookingAsync(Booking booking, string createdBy)
    {
        // 1. Sinh mã tr?c ti?p ? Memory d? không ph?i Update
        var timestamp = DateTime.UtcNow.ToString("yyMMddHHmmss");
        var rand = Random.Shared.Next(100, 999);
        
        booking.BookingCode = $"BKG{timestamp}{rand}";
        booking.Status = "Pending";
        booking.CreatedBy = createdBy;
        booking.CreatedAt = DateTime.UtcNow;

        var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
        if (nights <= 0) nights = 1;
        
        var requestedType = booking.RequestedRoomType?.Trim() ?? "";
        var requestedTypeLower = requestedType.ToLower();
        decimal roomPrice = 0;
        
        _logger.LogInformation($"? [CreateBookingAsync] Looking for room price. RequestedRoomType: '{requestedType}', Nights: {nights}");
        
        var roomTypes = await _context.RoomTypes
            .Where(rt => rt.TypeName.ToLower() == requestedTypeLower ||
                         rt.TypeCode.ToLower() == requestedTypeLower ||
                         rt.TypeName.ToLower().Replace(" room", "") == requestedTypeLower ||
                         requestedTypeLower.Replace(" room", "") == rt.TypeName.ToLower().Replace(" room", ""))
            .ToListAsync();
        
        if (roomTypes.Any())
        {
            roomPrice = roomTypes.FirstOrDefault()?.BasePrice ?? 0;
        }
        else
        {
            roomTypes = await _context.RoomTypes
                .Where(rt => rt.TypeName.ToLower().Contains(requestedTypeLower) ||
                             requestedTypeLower.Contains(rt.TypeName.ToLower().Replace(" room", "")) ||
                             rt.TypeName.ToLower().Replace(" room", "").Contains(requestedTypeLower))
                .ToListAsync();
            
            if (roomTypes.Any()) roomPrice = roomTypes.FirstOrDefault()?.BasePrice ?? 0;
        }
        
        if (roomPrice <= 0)
        {
            var rooms = await _context.Rooms
                .Where(r => r.RoomType.ToLower() == requestedTypeLower ||
                            r.RoomType.ToLower().Contains(requestedTypeLower) ||
                            requestedTypeLower.Contains(r.RoomType.ToLower()))
                .ToListAsync();
            
            if (rooms.Any()) roomPrice = rooms.FirstOrDefault()?.PricePerNight ?? 0;
        }
        
        booking.EstimatedTotalAmount = roomPrice > 0 ? roomPrice * nights : 0;
        
        // 2. B?C TRONG TRANSACTION Ð? Ð?M B?O TOÀN V?N D? LI?U VÀ GI?M SAVECHANGES
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync(); // Luu Booking d? l?y BookingId
            
            var subTotal = booking.EstimatedTotalAmount ?? 0;
            var taxRate = 10.0m;
            var taxAmount = subTotal * (taxRate / 100);
            var totalAmount = subTotal + taxAmount;

            var invoice = new Invoice
            {
                InvoiceNumber = $"INV{timestamp}{rand}",
                BookingId = booking.BookingId,
                CustomerId = booking.CustomerId,
                SubTotal = subTotal,
                TaxAmount = taxAmount,
                TaxRate = taxRate,
                TotalAmount = totalAmount,
                PaidAmount = 0,
                BalanceDue = totalAmount,
                Status = "Issued",
                IssueDate = DateTime.UtcNow,
                IssuedBy = createdBy
            };

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync(); // Luu Invoice

            await transaction.CommitAsync();
            _logger.LogInformation($"? Transaction committed successfully for Booking {booking.BookingCode}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"? Transaction failed for booking {booking.BookingCode}: {ex.Message}");
            throw;
        }

        await _auditService.LogAsync("Booking", booking.BookingId, "Create", createdBy, null, 
            $"Booking {booking.BookingCode} created");

        await _notificationService.CreateNotificationAsync("Info", "New Booking", 
            $"New booking {booking.BookingCode} created", "Medium", "FrontDesk", 
            null, "Booking", booking.BookingId);

        return booking;
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Room)
            .Include(b => b.Charges)
            .Include(b => b.Invoice)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);
    }

    public async Task<Booking?> GetBookingByCodeAsync(string bookingCode)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Room)
            .Include(b => b.Charges)
            .Include(b => b.Invoice)
            .FirstOrDefaultAsync(b => b.BookingCode == bookingCode);
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
    {
        if (_cache.TryGetValue("all_bookings", out IEnumerable<Booking> cached))
            return cached;

        var data = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Room)
            .OrderByDescending(b => b.CreatedAt)
            .Take(50)
            .ToListAsync();

        _cache.Set("all_bookings", data, TimeSpan.FromSeconds(30));
        return data;
    }

    public async Task<IEnumerable<Booking>> GetBookingsByCustomerAsync(int customerId)
    {
        return await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.Charges)
            .Include(b => b.Invoice)
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> TransferToFrontDeskAsync(int bookingId, string performedBy)
    {
        var booking = await GetBookingByIdAsync(bookingId);
        if (booking == null || booking.Status != "Pending")
            return false;

        booking.Status = "Confirmed";
        booking.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Booking", bookingId, "TransferToFrontDesk", performedBy, 
            "Pending", "Confirmed", "Booking transferred to front desk");

        return true;
    }

    public async Task<bool> AssignRoomAsync(int bookingId, int roomId, string performedBy)
    {
        var booking = await GetBookingByIdAsync(bookingId);
        if (booking == null)
            return false;

        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
        if (room == null || !room.IsAvailable || room.HousekeepingStatus != "Ready")
            return false;

        // Check for overlapping bookings (prevent double booking)
        var overlappingBookings = await _context.Bookings
            .Where(b => b.RoomId == roomId && 
                   (b.Status == "Assigned" || b.Status == "CheckedIn") &&
                   b.CheckInDate < booking.CheckOutDate &&
                   b.CheckOutDate > booking.CheckInDate)
            .AnyAsync();

        if (overlappingBookings)
            return false; // Room already booked for this period

        booking.RoomId = roomId;
        booking.Status = "Assigned";
        booking.UpdatedAt = DateTime.UtcNow;

        room.IsAvailable = false;
        
        _unitOfWork.Bookings.Update(booking);
        _unitOfWork.Rooms.Update(room);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Booking", bookingId, "AssignRoom", performedBy, 
            null, $"Room {room.RoomNumber}", $"Room {room.RoomNumber} assigned to booking");

        return true;
    }

    public async Task<bool> CheckInAsync(int bookingId, string performedBy)
    {
        var booking = await GetBookingByIdAsync(bookingId);
        if (booking == null || booking.Status != "Assigned")
            return false;

        booking.Status = "CheckedIn";
        booking.ActualCheckInTime = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        // Add room charges
        var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
        var room = await _unitOfWork.Rooms.GetByIdAsync(booking.RoomId!.Value);
        
        if (room != null)
        {
            var roomCharge = new Charge
            {
                BookingId = bookingId,
                RoomId = room.RoomId,
                ChargeType = "RoomCharge",
                Description = $"Room charges for {nights} night(s)",
                Amount = room.PricePerNight,
                Quantity = nights,
                TotalAmount = room.PricePerNight * nights,
                ChargeDate = DateTime.UtcNow,
                CreatedBy = performedBy
            };

            await _unitOfWork.Charges.AddAsync(roomCharge);
        }

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Booking", bookingId, "CheckIn", performedBy, 
            "Assigned", "CheckedIn", "Guest checked in");

        return true;
    }

    public async Task<bool> AddChargeAsync(int bookingId, Charge charge, string createdBy)
    {
        var booking = await GetBookingByIdAsync(bookingId);
        if (booking == null || booking.Status != "CheckedIn")
            return false;

        charge.BookingId = bookingId;
        charge.ChargeDate = DateTime.UtcNow;
        charge.CreatedBy = createdBy;
        charge.TotalAmount = charge.Amount * charge.Quantity;

        await _unitOfWork.Charges.AddAsync(charge);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Charge", charge.ChargeId, "Create", createdBy, 
            null, $"{charge.Description} - {charge.TotalAmount:C}", "Charge added to booking");

        return true;
    }

    public async Task<Invoice> CheckOutAsync(int bookingId, string performedBy)
    {
        var booking = await GetBookingByIdAsync(bookingId);
        if (booking == null || booking.Status != "CheckedIn")
            throw new InvalidOperationException("Booking not in CheckedIn status");

        // Update booking status
        booking.Status = "CheckedOut";
        booking.ActualCheckOutTime = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        // Update room availability
        if (booking.RoomId.HasValue)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(booking.RoomId.Value);
            if (room != null)
            {
                room.IsAvailable = false; // Will be cleaned
                room.HousekeepingStatus = "Dirty";
                _unitOfWork.Rooms.Update(room);
            }
        }

        // Calculate invoice
        var charges = booking.Charges.ToList();
        var subTotal = charges.Sum(c => c.TotalAmount);
        var taxRate = 10.0m;
        var taxAmount = subTotal * (taxRate / 100);
        var totalAmount = subTotal + taxAmount;

        // Generate invoice number
        var lastInvoice = (await _unitOfWork.Invoices.GetAllAsync())
            .OrderByDescending(i => i.InvoiceId)
            .FirstOrDefault();

        var invoiceNumber = lastInvoice != null ?
            int.Parse(lastInvoice.InvoiceNumber.Replace("INV", "")) + 1 : 1;

        var invoice = new Invoice
        {
            InvoiceNumber = $"INV{invoiceNumber:D7}",
            BookingId = bookingId,
            CustomerId = booking.CustomerId,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            TaxRate = taxRate,
            TotalAmount = totalAmount,
            BalanceDue = totalAmount,
            Status = "Issued",
            IssueDate = DateTime.UtcNow,
            IssuedBy = performedBy
        };

        await _unitOfWork.Invoices.AddAsync(invoice);
        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Booking", bookingId, "CheckOut", performedBy, 
            "CheckedIn", "CheckedOut", $"Guest checked out. Invoice {invoice.InvoiceNumber} issued");

        await _notificationService.CreateNotificationAsync("Info", "Checkout Completed", 
            $"Booking {booking.BookingCode} checked out. Invoice {invoice.InvoiceNumber} issued", 
            "Medium", "Cashier", null, "Invoice", invoice.InvoiceId);

        return invoice;
    }

    public async Task<bool> CancelBookingAsync(int bookingId, string reason, string performedBy)
    {
        var booking = await GetBookingByIdAsync(bookingId);
        if (booking == null || booking.Status == "CheckedOut" || booking.Status == "Cancelled")
            return false;

        booking.Status = "Cancelled";
        booking.CancellationReason = reason;
        booking.UpdatedAt = DateTime.UtcNow;

        // Release room if assigned
        if (booking.RoomId.HasValue)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(booking.RoomId.Value);
            if (room != null)
            {
                room.IsAvailable = true;
                _unitOfWork.Rooms.Update(room);
            }
        }

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Booking", bookingId, "Cancel", performedBy, 
            booking.Status, "Cancelled", $"Booking cancelled. Reason: {reason}");

        return true;
    }

    public async Task<bool> ProcessOnlinePaymentAsync(int bookingId, string performedBy)
    {
        _logger.LogInformation($"[ProcessOnlinePaymentAsync] ?? Processing payment for booking {bookingId} by {performedBy}");
        
        var booking = await GetBookingByIdAsync(bookingId);
        if (booking == null)
        {
            _logger.LogWarning($"[ProcessOnlinePaymentAsync] ? Booking {bookingId} not found");
            return false;
        }

        _logger.LogInformation($"[ProcessOnlinePaymentAsync] ?? Booking {bookingId} current status: '{booking.Status}', BookingCode: '{booking.BookingCode}'");

        // Không cho phép thanh toán n?u dã thanh toán r?i
        if (booking.Status == "Paid")
        {
            _logger.LogWarning($"[ProcessOnlinePaymentAsync] ?? Booking {bookingId} already paid");
            return false;
        }

        // Ch? cho phép thanh toán n?u booking dang ? tr?ng thái Pending ho?c Confirmed
        if (booking.Status != "Pending" && booking.Status != "Confirmed")
        {
            _logger.LogWarning($"[ProcessOnlinePaymentAsync] ?? Booking {bookingId} status is '{booking.Status}', cannot process payment");
            return false;
        }

        var oldStatus = booking.Status;
        _logger.LogInformation($"[ProcessOnlinePaymentAsync] ?? Updating booking {bookingId} status from '{oldStatus}' to 'Paid'");
        
        booking.Status = "Paid";
        booking.UpdatedAt = DateTime.UtcNow;

        Invoice? createdInvoice = null;
        
        // T?o invoice n?u chua có
        if (booking.Invoice == null)
        {
            var subTotal = booking.EstimatedTotalAmount ?? 0;
            var taxRate = 10.0m;
            var taxAmount = subTotal * (taxRate / 100);
            var totalAmount = subTotal + taxAmount;

            // Generate invoice number
            var lastInvoice = (await _unitOfWork.Invoices.GetAllAsync())
                .OrderByDescending(i => i.InvoiceId)
                .FirstOrDefault();

            var invoiceNumber = lastInvoice != null ?
                int.Parse(lastInvoice.InvoiceNumber.Replace("INV", "")) + 1 : 1;

            createdInvoice = new Invoice
            {
                InvoiceNumber = $"INV{invoiceNumber:D7}",
                BookingId = bookingId,
                CustomerId = booking.CustomerId,
                SubTotal = subTotal,
                TaxAmount = taxAmount,
                TaxRate = taxRate,
                TotalAmount = totalAmount,
                PaidAmount = totalAmount,
                BalanceDue = 0,
                Status = "Paid",
                IssueDate = DateTime.UtcNow,
                PaidDate = DateTime.UtcNow,
                IssuedBy = performedBy
            };

            await _unitOfWork.Invoices.AddAsync(createdInvoice);
        }
        else
        {
            // N?u dã có invoice, c?p nh?t tr?ng thái thanh toán
            var existingInvoice = booking.Invoice;
            existingInvoice.PaidAmount = existingInvoice.TotalAmount;
            existingInvoice.BalanceDue = 0;
            existingInvoice.Status = "Paid";
            existingInvoice.PaidDate = DateTime.UtcNow;
            _unitOfWork.Invoices.Update(existingInvoice);
            createdInvoice = existingInvoice;
        }

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation($"[ProcessOnlinePaymentAsync] ??? SUCCESS: Booking {bookingId} status updated to 'Paid'. Invoice: {createdInvoice?.InvoiceNumber ?? "N/A"}");

        await _auditService.LogAsync("Booking", bookingId, "PayOnline", performedBy, 
            oldStatus, "Paid", $"Online payment processed successfully. Invoice: {createdInvoice?.InvoiceNumber ?? "N/A"}");

        return true;
    }
}








