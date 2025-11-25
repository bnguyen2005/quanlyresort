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
        
        // Restaurant Orders đã thanh toán trong khoảng thời gian
        var restaurantOrdersList = await _context.RestaurantOrders
            .Where(o => o.PaymentStatus == "Paid" && 
                      ((o.UpdatedAt.HasValue && o.UpdatedAt.Value >= start && o.UpdatedAt.Value < end) ||
                       (!o.UpdatedAt.HasValue && o.CreatedAt >= start && o.CreatedAt < end)))
            .Select(o => o.TotalAmount)
            .ToListAsync();
        var totalRestaurantOrders = restaurantOrdersList.Sum(o => (decimal?)o) ?? 0;
        
        // Tổng doanh thu = invoices + restaurant orders
        var totalRevenue = totalInvoices + totalRestaurantOrders;

        // Revenue by type - client-side aggregation
        // Include Invoices (Bookings) and Restaurant Orders
        var revenueByType = invoicesInRange.Select(i => new { Type = "Đặt phòng", Amount = i })
            .Concat(restaurantOrdersList.Select(r => new { Type = "Nhà hàng", Amount = r }))
            .GroupBy(r => r.Type)
            .Select(g => new
            {
                type = g.Key,
                amount = g.Sum(r => (decimal?)r.Amount) ?? 0,
                count = g.Count()
            })
            .OrderByDescending(r => r.amount)
            .ToList();

        // Daily revenue trend - dựa trên invoices + restaurant
        // Load all paid invoices first, then filter by date range
        var allPaidInvoices = await _context.Invoices
            .Where(i => i.PaidDate.HasValue && i.Status == "Paid")
            .Select(i => new { PaidDate = i.PaidDate!.Value, i.PaidAmount })
            .ToListAsync();
        
        var dailyInvoicesList = allPaidInvoices
            .Where(i => i.PaidDate >= start && i.PaidDate < end)
            .ToList();
        
        // Restaurant Orders by date
        var dailyRestaurantOrdersList = await _context.RestaurantOrders
            .Where(o => o.PaymentStatus == "Paid" &&
                      ((o.UpdatedAt.HasValue && o.UpdatedAt.Value >= start && o.UpdatedAt.Value < end) ||
                       (!o.UpdatedAt.HasValue && o.CreatedAt >= start && o.CreatedAt < end)))
            .Select(o => new { 
                Date = o.UpdatedAt.HasValue ? o.UpdatedAt.Value.Date : o.CreatedAt.Date, 
                Amount = o.TotalAmount 
            })
            .ToListAsync();
        
        // Combine invoices và restaurant orders theo ngày
        var dailyData = dailyInvoicesList
            .Select(i => new { Date = i.PaidDate.Date, Amount = i.PaidAmount })
            .Concat(dailyRestaurantOrdersList.Select(r => new { Date = r.Date, Amount = r.Amount }))
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
            totalInvoices,
            totalRestaurantOrders,
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
        
        // Load stay data from invoices (only Paid invoices with assigned rooms)
        var invoiceStays = await _context.Invoices
            .Where(i => i.Status == "Paid" && i.Booking != null && i.Booking.RoomId.HasValue)
            .Select(i => new 
            { 
                RoomId = i.Booking!.RoomId!.Value,
                CheckIn = i.Booking.CheckInDate.Date,
                CheckOut = i.Booking.CheckOutDate.Date
            })
            .ToListAsync();
        
        // Daily occupancy
        var dailyOccupancy = new List<object>();
        for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            var occupiedRooms = invoiceStays
                .Where(s => s.CheckIn <= date && s.CheckOut > date)
                .Select(s => s.RoomId)
                .Distinct()
                .Count();

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

        // Top customers by spending - dựa trên invoices + restaurant orders đã thanh toán
        var paidInvoices = await _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status == "Paid" &&
                        i.CustomerId > 0 &&
                        i.PaidDate.HasValue &&
                        i.PaidDate.Value >= start &&
                        i.PaidDate.Value < end)
            .Select(i => new
            {
                i.CustomerId,
                CustomerName = i.Customer!.FullName,
                Amount = i.PaidAmount > 0 ? i.PaidAmount : i.TotalAmount,
                TransactionDate = i.PaidDate ?? i.IssueDate
            })
            .ToListAsync();

        var paidRestaurantOrders = await _context.RestaurantOrders
            .Include(o => o.Customer)
            .Where(o => o.PaymentStatus == "Paid" &&
                        o.CustomerId.HasValue &&
                        ((o.UpdatedAt.HasValue && o.UpdatedAt.Value >= start && o.UpdatedAt.Value < end) ||
                         (!o.UpdatedAt.HasValue && o.CreatedAt >= start && o.CreatedAt < end)))
            .Select(o => new
            {
                CustomerId = o.CustomerId!.Value,
                CustomerName = o.Customer != null ? o.Customer.FullName : "Khách hàng nhà hàng",
                Amount = o.TotalAmount,
                TransactionDate = o.UpdatedAt ?? o.CreatedAt
            })
            .ToListAsync();

        var combinedCustomerSpending = paidInvoices
            .Concat(paidRestaurantOrders)
            .GroupBy(c => new { c.CustomerId, c.CustomerName })
            .Select(g => new
            {
                customerId = g.Key.CustomerId,
                customerName = g.Key.CustomerName,
                totalSpent = g.Sum(x => (decimal?)x.Amount) ?? 0,
                bookingCount = g.Count(),
                lastBooking = g.Max(x => x.TransactionDate)
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
            topCustomers = combinedCustomerSpending,
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

        var todayLocal = DateTime.Today;
        var todayStartLocal = todayLocal;
        var todayEndLocal = todayLocal.AddDays(1);
        var todayUtc = DateTime.UtcNow.Date;
        
        // Invoices - PaidDate is stored as UTC, so convert and compare to local today
        var paidInvoicesToday = await _context.Invoices
            .Where(i => i.Status == "Paid" && i.PaidDate.HasValue)
            .Select(i => new 
            { 
                i.PaidDate, 
                Amount = i.PaidAmount > 0 ? i.PaidAmount : i.TotalAmount 
            })
            .ToListAsync();
        
        var invoicesRevenue = paidInvoicesToday
            .Where(i => TimeZoneInfo.ConvertTimeFromUtc(i.PaidDate!.Value, TimeZoneInfo.Local).Date == todayLocal)
            .Sum(i => (decimal?)i.Amount) ?? 0;
        
        // Restaurant Orders đã thanh toán hôm nay
        var todayRestaurantOrdersList = await _context.RestaurantOrders
            .Where(o => o.PaymentStatus == "Paid" && 
                      ((o.UpdatedAt.HasValue && o.UpdatedAt.Value.Date == todayLocal) ||
                       (!o.UpdatedAt.HasValue && o.CreatedAt.Date == todayLocal)))
            .Select(o => o.TotalAmount)
            .ToListAsync();
        var restaurantRevenue = todayRestaurantOrdersList.Sum(o => (decimal?)o) ?? 0;
        
        // Tổng doanh thu hôm nay = Invoices + Restaurant Orders
        var todayRevenue = invoicesRevenue + restaurantRevenue;

        // Calculate today occupancy dựa trên invoices + rooms
        var totalRooms = await _context.Rooms.CountAsync();
        var occupiedRooms = await _context.Invoices
            .Where(i => i.Status == "Paid" &&
                   i.Booking != null &&
                   i.Booking.CheckInDate <= todayLocal &&
                   i.Booking.CheckOutDate > todayLocal)
            .Select(i => i.Booking!.RoomId)
            .Where(roomId => roomId.HasValue)
            .Select(roomId => roomId!.Value)
            .Distinct()
            .CountAsync();
        var todayOccupancyRate = totalRooms > 0 ? (decimal)occupiedRooms / totalRooms * 100 : 0;

        // This month stats - dựa trên invoices + restaurant
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
        
        var thisMonthRevenue = thisMonthInvoicesRevenue + thisMonthRestaurantRevenue;

        // Last month stats
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
        
        var lastMonthRevenue = lastMonthInvoicesRevenue + lastMonthRestaurantRevenue;

        var revenueGrowth = lastMonthRevenue > 0 
            ? Math.Round((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100, 2)
            : 0;

        // Today's bookings - dựa trên invoices phát hành hôm nay
        var todayBookings = await _context.Invoices
            .Where(i => i.Status != "Cancelled" &&
                        i.IssueDate >= todayStartLocal &&
                        i.IssueDate < todayEndLocal)
            .CountAsync();

        // Today's check-ins - dựa trên thông tin check-in của booking gắn với invoice
        var todayCheckIns = await _context.Invoices
            .Where(i => i.Status == "Paid" &&
                        i.Booking != null &&
                        i.Booking.CheckInDate >= todayStartLocal &&
                        i.Booking.CheckInDate < todayEndLocal)
            .CountAsync();

        // Invoice-based booking stats
        var totalBookings = await _context.Invoices
            .Where(i => i.Status != "Cancelled")
            .CountAsync();

        var activeBookings = await _context.Invoices
            .Where(i => i.Status == "Paid" &&
                        i.Booking != null &&
                        i.Booking.CheckInDate <= todayLocal &&
                        i.Booking.CheckOutDate > todayLocal)
            .CountAsync();

        var pendingBookings = await _context.Invoices
            .Where(i => i.Status == "Issued" || i.Status == "PartiallyPaid")
            .CountAsync();

        // This month vs last month invoice counts
        var thisMonthBookings = await _context.Invoices
            .Where(i => i.Status != "Cancelled" &&
                        i.IssueDate >= thisMonth &&
                        i.IssueDate < thisMonth.AddMonths(1))
            .CountAsync();

        var lastMonthBookings = await _context.Invoices
            .Where(i => i.Status != "Cancelled" &&
                        i.IssueDate >= lastMonth &&
                        i.IssueDate < thisMonth)
            .CountAsync();

        var bookingGrowth = lastMonthBookings > 0 
            ? Math.Round((double)(thisMonthBookings - lastMonthBookings) / lastMonthBookings * 100, 2)
            : 0;

        return Ok(new
        {
            today = new
            {
                bookings = todayBookings,
                revenue = todayRevenue,
                checkIns = todayCheckIns
            },
            occupancy = new
            {
                rate = Math.Round(todayOccupancyRate, 2),
                occupied = occupiedRooms,
                total = totalRooms,
                available = totalRooms - occupiedRooms
            },
            growth = new
            {
                revenue = revenueGrowth,
                bookings = bookingGrowth
            },
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

