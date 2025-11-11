using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyResort.Data;
using QuanLyResort.Models;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,Accounting")]
public class ReportsController : ControllerBase
{
    private readonly ResortDbContext _context;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(ResortDbContext context, ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("daily-occupancy")]
    public async Task<IActionResult> GetDailyOccupancy([FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.Today;
        
        var totalRooms = await _context.Rooms.CountAsync();
        var occupiedRooms = await _context.Bookings
            .Where(b => b.Status == "CheckedIn" &&
                   b.CheckInDate <= targetDate &&
                   b.CheckOutDate > targetDate)
            .CountAsync();

        var occupancyRate = totalRooms > 0 ? (decimal)occupiedRooms / totalRooms * 100 : 0;

        return Ok(new
        {
            date = targetDate,
            totalRooms,
            occupiedRooms,
            availableRooms = totalRooms - occupiedRooms,
            occupancyRate = Math.Round(occupancyRate, 2)
        });
    }

    [HttpGet("daily-revenue")]
    public async Task<IActionResult> GetDailyRevenue([FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.Today;
        var startOfDay = targetDate.Date;
        var endOfDay = startOfDay.AddDays(1);

        var charges = await _context.Charges
            .Where(c => c.ChargeDate >= startOfDay && c.ChargeDate < endOfDay)
            .ToListAsync();

        // Client-side aggregation (already loaded to memory)
        var chargesRevenue = charges.Sum(c => (decimal?)c.TotalAmount) ?? 0;
        var roomRevenue = charges.Where(c => c.ChargeType == "RoomCharge").Sum(c => (decimal?)c.TotalAmount) ?? 0;
        var serviceRevenue = charges.Where(c => c.ChargeType == "ServiceCharge").Sum(c => (decimal?)c.TotalAmount) ?? 0;
        
        // Invoices đã thanh toán trong ngày
        // Load all paid invoices first, then filter by date in memory (handle UTC conversion)
        var dayInvoicesList = await _context.Invoices
            .Where(i => i.PaidDate.HasValue && i.Status == "Paid")
            .Select(i => new { i.PaidDate, i.PaidAmount })
            .ToListAsync();
        
        var invoicesToday = dayInvoicesList
            .Where(i => i.PaidDate.HasValue && 
                   i.PaidDate.Value.Date >= startOfDay && 
                   i.PaidDate.Value.Date < endOfDay)
            .Select(i => i.PaidAmount)
            .ToList();
        var invoicesRevenue = invoicesToday.Sum(i => (decimal?)i) ?? 0;
        
        // Restaurant Orders đã thanh toán trong ngày
        var restaurantOrdersToday = await _context.RestaurantOrders
            .Where(o => o.PaymentStatus == "Paid" &&
                       ((o.UpdatedAt.HasValue && o.UpdatedAt.Value.Date == targetDate) ||
                        (!o.UpdatedAt.HasValue && o.CreatedAt.Date == targetDate)))
            .Select(o => o.TotalAmount)
            .ToListAsync();
        var restaurantRevenue = restaurantOrdersToday.Sum(o => (decimal?)o) ?? 0;
        
        // Tổng doanh thu = charges + invoices + restaurant orders
        var totalRevenue = chargesRevenue + invoicesRevenue + restaurantRevenue;

        // SQLite không hỗ trợ SumAsync trên decimal trực tiếp -> chuyển sang client-side aggregation
        var paymentsList = await _context.Invoices
            .Where(i => i.PaidDate.HasValue && 
                   i.PaidDate.Value >= startOfDay && 
                   i.PaidDate.Value < endOfDay)
            .Select(i => i.PaidAmount)
            .ToListAsync();
        var payments = paymentsList.Sum(i => (decimal?)i) ?? 0;

        return Ok(new
        {
            date = targetDate,
            totalRevenue,
            roomRevenue,
            serviceRevenue,
            restaurantRevenue,
            totalPayments = payments,
            breakdown = charges.GroupBy(c => c.ChargeType).Select(g => new
            {
                chargeType = g.Key,
                amount = g.Sum(c => (decimal?)c.TotalAmount) ?? 0,
                count = g.Count()
            })
        });
    }

    [HttpGet("service-usage")]
    public async Task<IActionResult> GetServiceUsage([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today.AddDays(1); // Include end date

        // Doanh thu dịch vụ từ 2 nguồn:
        // 1. Charges table (service charges được thêm vào booking)
        // 2. RestaurantOrders table (đơn đặt món nhà hàng đã thanh toán)
        
        // 1. Service charges từ Charges table
        var serviceChargesList = await _context.Charges
            .Include(c => c.Service)
            .Where(c => c.ChargeDate >= start && c.ChargeDate < end && c.ServiceId.HasValue)
            .Select(c => new { ServiceId = (int?)c.ServiceId, ServiceName = c.Service!.ServiceName, Quantity = c.Quantity, TotalAmount = c.TotalAmount })
            .ToListAsync();
        
        // 2. Restaurant orders đã thanh toán trong khoảng thời gian
        // Load all paid restaurant orders, then filter by date in memory (handle UTC conversion)
        var allRestaurantOrders = await _context.RestaurantOrders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Service)
            .Where(o => o.PaymentStatus == "Paid" && o.Status != "Cancelled")
            .ToListAsync();
        
        // Filter orders by CreatedAt date (convert UTC to local)
        var restaurantOrdersInRange = allRestaurantOrders
            .Where(o => {
                var orderDateLocal = TimeZoneInfo.ConvertTimeFromUtc(o.CreatedAt, TimeZoneInfo.Local).Date;
                return orderDateLocal >= start.Date && orderDateLocal < end.Date;
            })
            .ToList();
        
        // Extract service data from restaurant order items - use same structure as charges
        var restaurantServiceData = restaurantOrdersInRange
            .SelectMany(o => o.OrderItems.Select(oi => new {
                ServiceId = (int?)oi.ServiceId,
                ServiceName = oi.Service.ServiceName,
                Quantity = oi.Quantity,
                TotalAmount = oi.SubTotal
            }))
            .ToList();
        
        // Combine charges and restaurant orders - now they have same anonymous type structure
        var allServiceData = serviceChargesList
            .Concat(restaurantServiceData)
            .GroupBy(s => new { s.ServiceId, s.ServiceName })
            .Select(g => new
            {
                serviceId = g.Key.ServiceId,
                serviceName = g.Key.ServiceName,
                totalUsage = g.Sum(s => s.Quantity),
                totalRevenue = g.Sum(s => (decimal?)s.TotalAmount) ?? 0,
                transactionCount = g.Count()
            })
            .OrderByDescending(x => x.totalRevenue)
            .ToList();

        // Debug logging
        _logger.LogInformation($"Service usage calculation - Charges: {serviceChargesList.Count}, Restaurant Orders: {restaurantOrdersInRange.Count}, Total Services: {allServiceData.Count}");

        return Ok(new
        {
            startDate = start,
            endDate = end.AddDays(-1),
            services = allServiceData
        });
    }

    [HttpGet("revenue-summary")]
    public async Task<IActionResult> GetRevenueSummary([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today.AddDays(1);

        // Total revenue from charges and invoices
        // SQLite không hỗ trợ SumAsync trên decimal trực tiếp -> chuyển sang client-side aggregation
        var chargesList = await _context.Charges
            .Where(c => c.ChargeDate >= start && c.ChargeDate < end)
            .Select(c => c.TotalAmount)
            .ToListAsync();
        var totalCharges = chargesList.Sum(c => (decimal?)c) ?? 0;
        
        // Invoices đã thanh toán trong khoảng thời gian
        // Load all paid invoices first, then filter by date in memory (handle UTC conversion)
        var invoicesList = await _context.Invoices
            .Where(i => i.PaidDate.HasValue && i.Status == "Paid")
            .Select(i => new { i.PaidDate, i.PaidAmount })
            .ToListAsync();
        
        var invoicesInRange = invoicesList
            .Where(i => i.PaidDate.HasValue && 
                   i.PaidDate.Value >= start && 
                   i.PaidDate.Value < end)
            .Select(i => i.PaidAmount)
            .ToList();
        var totalInvoices = invoicesInRange.Sum(i => (decimal?)i) ?? 0;
        
        // Tổng doanh thu = charges + invoices
        var totalRevenue = totalCharges + totalInvoices;

        // Revenue by type - client-side aggregation
        var chargesByTypeList = await _context.Charges
            .Where(c => c.ChargeDate >= start && c.ChargeDate < end)
            .Select(c => new { c.ChargeType, c.TotalAmount })
            .ToListAsync();
        
        var revenueByType = chargesByTypeList
            .GroupBy(c => c.ChargeType)
            .Select(g => new
            {
                type = g.Key,
                amount = g.Sum(c => (decimal?)c.TotalAmount) ?? 0,
                count = g.Count()
            })
            .ToList();

        // Daily revenue trend - client-side aggregation (charges + invoices)
        var dailyChargesList = await _context.Charges
            .Where(c => c.ChargeDate >= start && c.ChargeDate < end)
            .Select(c => new { c.ChargeDate, c.TotalAmount })
            .ToListAsync();
        
        // Load all paid invoices first, then filter by date range
        var allPaidInvoices = await _context.Invoices
            .Where(i => i.PaidDate.HasValue && i.Status == "Paid")
            .Select(i => new { PaidDate = i.PaidDate!.Value, i.PaidAmount })
            .ToListAsync();
        
        var dailyInvoicesList = allPaidInvoices
            .Where(i => i.PaidDate >= start && i.PaidDate < end)
            .ToList();
        
        // Combine charges and invoices by date
        var dailyData = dailyChargesList
            .Select(c => new { Date = c.ChargeDate.Date, Amount = c.TotalAmount })
            .Concat(dailyInvoicesList.Select(i => new { Date = i.PaidDate.Date, Amount = i.PaidAmount }))
            .GroupBy(d => d.Date)
            .Select(g => new
            {
                date = g.Key,
                amount = g.Sum(d => (decimal?)d.Amount) ?? 0,
                transactions = g.Count()
            })
            .OrderBy(x => x.date)
            .ToList();
        
        var dailyRevenue = dailyData;

        // Payments received - client-side aggregation
        var paymentsList = await _context.Invoices
            .Where(i => i.PaidDate.HasValue && i.PaidDate >= start && i.PaidDate < end)
            .Select(i => i.PaidAmount)
            .ToListAsync();
        var totalPayments = paymentsList.Sum(i => (decimal?)i) ?? 0;

        return Ok(new
        {
            startDate = start,
            endDate = end.AddDays(-1),
            totalCharges,
            totalInvoices,
            totalRevenue,
            totalPayments,
            revenueByType,
            dailyRevenue
        });
    }

    [HttpGet("occupancy-report")]
    public async Task<IActionResult> GetOccupancyReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;

        var totalRooms = await _context.Rooms.CountAsync();
        
        // Daily occupancy
        var dailyOccupancy = new List<object>();
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var occupiedRooms = await _context.Bookings
                .Where(b => b.Status == "CheckedIn" &&
                       b.CheckInDate <= date &&
                       b.CheckOutDate > date)
                .CountAsync();

            var occupancyRate = totalRooms > 0 ? (decimal)occupiedRooms / totalRooms * 100 : 0;

            dailyOccupancy.Add(new
            {
                date,
                totalRooms,
                occupiedRooms,
                availableRooms = totalRooms - occupiedRooms,
                occupancyRate = Math.Round(occupancyRate, 2)
            });
        }

        // Average occupancy
        var avgOccupancy = dailyOccupancy.Count > 0 
            ? Math.Round(dailyOccupancy.Average(d => (decimal)((dynamic)d).occupancyRate), 2)
            : 0;

        return Ok(new
        {
            startDate = start,
            endDate = end,
            totalRooms,
            averageOccupancy = avgOccupancy,
            dailyOccupancy
        });
    }

    [HttpGet("customer-analytics")]
    public async Task<IActionResult> GetCustomerAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today.AddDays(1);

        // Top customers by spending - client-side aggregation
        var chargesList = await _context.Charges
            .Include(c => c.Booking)
            .ThenInclude(b => b.Customer)
            .Where(c => c.ChargeDate >= start && c.ChargeDate < end && c.Booking != null)
            .Select(c => new { 
                c.Booking!.CustomerId, 
                CustomerName = c.Booking.Customer!.FullName,
                c.BookingId,
                c.TotalAmount,
                c.ChargeDate
            })
            .ToListAsync();
        
        var topCustomers = chargesList
            .GroupBy(c => new { c.CustomerId, c.CustomerName })
            .Select(g => new
            {
                customerId = g.Key.CustomerId,
                customerName = g.Key.CustomerName,
                totalSpent = g.Sum(c => (decimal?)c.TotalAmount) ?? 0,
                bookingCount = g.Select(c => c.BookingId).Distinct().Count(),
                lastBooking = g.Max(c => c.ChargeDate)
            })
            .OrderByDescending(x => x.totalSpent)
            .Take(10)
            .ToList();

        // Customer types distribution - client-side aggregation
        var allCustomers = await _context.Customers
            .Select(c => c.CustomerType)
            .ToListAsync();
        
        var totalCustomers = allCustomers.Count;
        var customerTypes = allCustomers
            .GroupBy(c => c)
            .Select(g => new
            {
                type = g.Key,
                count = g.Count(),
                percentage = totalCustomers > 0 ? Math.Round((decimal)g.Count() / totalCustomers * 100, 2) : 0
            })
            .ToList();

        // New customers
        var newCustomers = await _context.Customers
            .Where(c => c.CreatedAt >= start && c.CreatedAt < end)
            .CountAsync();

        return Ok(new
        {
            startDate = start,
            endDate = end.AddDays(-1),
            topCustomers,
            customerTypes,
            newCustomers
        });
    }

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var today = DateTime.Today;
        var thisMonth = new DateTime(today.Year, today.Month, 1);
        var lastMonth = thisMonth.AddMonths(-1);

        // Today's stats
        // Doanh thu từ 2 nguồn:
        // 1. Charges (room charges, service charges sau khi check-in)
        // 2. Invoices với PaidDate = today (bookings đã thanh toán online)
        // Note: PaidDate được lưu là UTC, cần so sánh với UTC date range
        var todayUtc = DateTime.UtcNow.Date;
        var todayStartUtc = todayUtc;
        var todayEndUtc = todayUtc.AddDays(1);
        
        // Also check local date range for charges (ChargeDate might be local)
        var todayLocal = DateTime.Today;
        var todayStartLocal = todayLocal;
        var todayEndLocal = todayLocal.AddDays(1);
        
        // Charges - check both UTC and local ranges
        var todayChargesList = await _context.Charges
            .Where(c => (c.ChargeDate >= todayStartUtc && c.ChargeDate < todayEndUtc) ||
                       (c.ChargeDate >= todayStartLocal && c.ChargeDate < todayEndLocal))
            .Select(c => c.TotalAmount)
            .ToListAsync();
        var chargesRevenue = todayChargesList.Sum(c => (decimal?)c) ?? 0;
        
        // Invoices - PaidDate is stored as UTC, so compare with UTC date range
        // But also check if it falls within today's local date when converted
        var allPaidInvoices = await _context.Invoices
            .Where(i => i.PaidDate.HasValue && i.Status == "Paid")
            .Select(i => new { i.PaidDate, i.PaidAmount })
            .ToListAsync();
        
        // Filter invoices: check if PaidDate (UTC) falls within today
        // Since PaidDate is UTC, convert to local time for comparison
        var invoicesToday = allPaidInvoices
            .Where(i => i.PaidDate.HasValue)
            .Where(i => {
                var paidDate = i.PaidDate!.Value;
                // Convert UTC to local time and check if date matches today
                var paidDateLocal = TimeZoneInfo.ConvertTimeFromUtc(paidDate, TimeZoneInfo.Local).Date;
                return paidDateLocal == todayLocal;
            })
            .Select(i => i.PaidAmount)
            .ToList();
        var invoicesRevenue = invoicesToday.Sum(i => (decimal?)i) ?? 0;
        
        // Restaurant Orders đã thanh toán hôm nay
        // Tính từ UpdatedAt hoặc CreatedAt nếu PaymentStatus = "Paid"
        var todayRestaurantOrdersList = await _context.RestaurantOrders
            .Where(o => o.PaymentStatus == "Paid" && 
                      ((o.UpdatedAt.HasValue && o.UpdatedAt.Value.Date == todayLocal) ||
                       (!o.UpdatedAt.HasValue && o.CreatedAt.Date == todayLocal)))
            .Select(o => o.TotalAmount)
            .ToListAsync();
        var restaurantRevenue = todayRestaurantOrdersList.Sum(o => (decimal?)o) ?? 0;
        
        // Debug logging
        _logger.LogInformation($"Today revenue calculation - Charges: {chargesRevenue}, Invoices: {invoicesRevenue}, Restaurant: {restaurantRevenue}, Total: {chargesRevenue + invoicesRevenue + restaurantRevenue}");
        _logger.LogInformation($"Today UTC: {todayUtc}, Today Local: {todayLocal}");
        _logger.LogInformation($"Total paid invoices: {allPaidInvoices.Count}, Invoices today: {invoicesToday.Count}, Restaurant orders today: {todayRestaurantOrdersList.Count}");
        
        // Tổng doanh thu hôm nay = Charges + Invoices + Restaurant Orders
        var todayRevenue = chargesRevenue + invoicesRevenue + restaurantRevenue;

        // Calculate today occupancy directly
        var totalRooms = await _context.Rooms.CountAsync();
        var occupiedRooms = await _context.Bookings
            .Where(b => b.Status == "CheckedIn" &&
                   b.CheckInDate <= today &&
                   b.CheckOutDate > today)
            .CountAsync();
        var todayOccupancyRate = totalRooms > 0 ? (decimal)occupiedRooms / totalRooms * 100 : 0;

        // This month stats - client-side aggregation
        // Tính từ Charges, Invoices, và Restaurant Orders
        var thisMonthChargesList = await _context.Charges
            .Where(c => c.ChargeDate >= thisMonth && c.ChargeDate < thisMonth.AddMonths(1))
            .Select(c => c.TotalAmount)
            .ToListAsync();
        var thisMonthChargesRevenue = thisMonthChargesList.Sum(c => (decimal?)c) ?? 0;
        
        // Invoices tháng này
        var thisMonthInvoicesList = await _context.Invoices
            .Where(i => i.PaidDate.HasValue && 
                       i.PaidDate.Value >= thisMonth && 
                       i.PaidDate.Value < thisMonth.AddMonths(1) &&
                       i.Status == "Paid")
            .Select(i => i.PaidAmount > 0 ? i.PaidAmount : i.TotalAmount)
            .ToListAsync();
        var thisMonthInvoicesRevenue = thisMonthInvoicesList.Sum(i => (decimal?)i) ?? 0;
        
        // Restaurant Orders tháng này
        var thisMonthRestaurantOrdersList = await _context.RestaurantOrders
            .Where(o => o.PaymentStatus == "Paid" &&
                       ((o.UpdatedAt.HasValue && o.UpdatedAt.Value >= thisMonth && o.UpdatedAt.Value < thisMonth.AddMonths(1)) ||
                        (!o.UpdatedAt.HasValue && o.CreatedAt >= thisMonth && o.CreatedAt < thisMonth.AddMonths(1))))
            .Select(o => o.TotalAmount)
            .ToListAsync();
        var thisMonthRestaurantRevenue = thisMonthRestaurantOrdersList.Sum(o => (decimal?)o) ?? 0;
        
        var thisMonthRevenue = thisMonthChargesRevenue + thisMonthInvoicesRevenue + thisMonthRestaurantRevenue;

        // Last month stats
        var lastMonthChargesList = await _context.Charges
            .Where(c => c.ChargeDate >= lastMonth && c.ChargeDate < thisMonth)
            .Select(c => c.TotalAmount)
            .ToListAsync();
        var lastMonthChargesRevenue = lastMonthChargesList.Sum(c => (decimal?)c) ?? 0;
        
        // Invoices tháng trước
        var lastMonthInvoicesList = await _context.Invoices
            .Where(i => i.PaidDate.HasValue && 
                       i.PaidDate.Value >= lastMonth && 
                       i.PaidDate.Value < thisMonth &&
                       i.Status == "Paid")
            .Select(i => i.PaidAmount > 0 ? i.PaidAmount : i.TotalAmount)
            .ToListAsync();
        var lastMonthInvoicesRevenue = lastMonthInvoicesList.Sum(i => (decimal?)i) ?? 0;
        
        // Restaurant Orders tháng trước
        var lastMonthRestaurantOrdersList = await _context.RestaurantOrders
            .Where(o => o.PaymentStatus == "Paid" &&
                       ((o.UpdatedAt.HasValue && o.UpdatedAt.Value >= lastMonth && o.UpdatedAt.Value < thisMonth) ||
                        (!o.UpdatedAt.HasValue && o.CreatedAt >= lastMonth && o.CreatedAt < thisMonth)))
            .Select(o => o.TotalAmount)
            .ToListAsync();
        var lastMonthRestaurantRevenue = lastMonthRestaurantOrdersList.Sum(o => (decimal?)o) ?? 0;
        
        var lastMonthRevenue = lastMonthChargesRevenue + lastMonthInvoicesRevenue + lastMonthRestaurantRevenue;

        var revenueGrowth = lastMonthRevenue > 0 
            ? Math.Round((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100, 2)
            : 0;

        // Bookings stats
        var totalBookings = await _context.Bookings.CountAsync();
        var activeBookings = await _context.Bookings.CountAsync(b => b.Status == "CheckedIn");
        var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == "Confirmed");

        return Ok(new
        {
            todayRevenue,
            todayOccupancy = Math.Round(todayOccupancyRate, 2),
            thisMonthRevenue,
            revenueGrowth,
            totalBookings,
            activeBookings,
            pendingBookings
        });
    }
}

