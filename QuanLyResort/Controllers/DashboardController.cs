using QuanLyResort.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;

namespace QuanLyResort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
    private ResortDbContext _context => _unitOfWork.Context;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/dashboard/stats
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetDashboardStats()
        {
            try
            {
                var today = DateTime.Today;
                var todayStart = today;
                var todayEnd = today.AddDays(1);
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                var lastMonth = thisMonth.AddMonths(-1);

                // Today's bookings - d?t phòng du?c t?o hôm nay (không ph?i check-in hôm nay)
                var todayBookings = await _context.Bookings
                    .Where(b => b.CreatedAt.Date == today && b.Status != "Cancelled")
                    .CountAsync();

                // Today's revenue - tính t?:
                // 1. Invoices dã thanh toán hôm nay (PaidDate = today, Status = Paid)
                // 2. Bookings có Status = "Paid" và UpdatedAt = today (dã thanh toán qua PayOs)
                // 3. Charges dã thanh toán hôm nay (ChargeDate = today)
                var todayInvoicesList = await _context.Invoices
                    .Where(i => i.PaidDate.HasValue && 
                               i.PaidDate.Value.Date == today && 
                               i.Status == "Paid")
                    .Select(i => i.PaidAmount > 0 ? i.PaidAmount : i.TotalAmount)
                    .ToListAsync();
                
                // Bookings dã thanh toán hôm nay (Status = "Paid" sau khi webhook x? lý)
                var todayPaidBookingsList = await _context.Bookings
                    .Where(b => b.UpdatedAt.HasValue && 
                               b.UpdatedAt.Value.Date == today && 
                               b.Status == "Paid" &&
                               b.EstimatedTotalAmount.HasValue &&
                               b.EstimatedTotalAmount > 0)
                    .Select(b => b.EstimatedTotalAmount!.Value)
                    .ToListAsync();
                
                // Charges (d?ch v?) dã thanh toán hôm nay
                var todayChargesList = await _context.Charges
                    .Where(c => c.ChargeDate.Date == today)
                    .Select(c => c.TotalAmount)
                    .ToListAsync();
                
                var todayRevenue = (todayInvoicesList.Sum(i => (decimal?)i) ?? 0) + 
                                  (todayPaidBookingsList.Sum(b => (decimal?)b) ?? 0) +
                                  (todayChargesList.Sum(c => (decimal?)c) ?? 0);

                // Today's check-ins - bookings có CheckInDate = today và dã check-in
                var todayCheckIns = await _context.Bookings
                    .Where(b => b.CheckInDate.Date == today && 
                               (b.Status == "CheckedIn" || b.Status == "Confirmed"))
                    .CountAsync();

                // Today's check-outs
                var todayCheckOuts = await _context.Bookings
                    .Where(b => b.CheckOutDate.Date == today && b.Status == "CheckedOut")
                    .CountAsync();

                // Current occupancy
                var totalRooms = await _context.Rooms.CountAsync(r => r.IsAvailable);
                var occupiedRooms = await _context.Bookings
                    .Where(b => b.CheckInDate <= DateTime.Now && 
                               b.CheckOutDate > DateTime.Now && 
                               b.Status == "CheckedIn")
                    .CountAsync();

                var occupancyRate = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0;

                // This month vs last month revenue - tính t?:
                // 1. Invoices dã thanh toán (PaidDate, Status = Paid)
                // 2. Bookings dã thanh toán (Status = Paid, UpdatedAt trong tháng)
                // 3. Charges trong tháng
                var thisMonthInvoicesList = await _context.Invoices
                    .Where(i => i.PaidDate.HasValue && 
                               i.PaidDate.Value >= thisMonth && 
                               i.Status == "Paid")
                    .Select(i => i.PaidAmount > 0 ? i.PaidAmount : i.TotalAmount)
                    .ToListAsync();
                
                var thisMonthPaidBookingsList = await _context.Bookings
                    .Where(b => b.UpdatedAt.HasValue && 
                               b.UpdatedAt.Value >= thisMonth && 
                               b.Status == "Paid" &&
                               b.EstimatedTotalAmount.HasValue &&
                               b.EstimatedTotalAmount > 0)
                    .Select(b => b.EstimatedTotalAmount!.Value)
                    .ToListAsync();
                
                var thisMonthChargesList = await _context.Charges
                    .Where(c => c.ChargeDate >= thisMonth)
                    .Select(c => c.TotalAmount)
                    .ToListAsync();
                
                var thisMonthRevenue = (thisMonthInvoicesList.Sum(i => (decimal?)i) ?? 0) +
                                      (thisMonthPaidBookingsList.Sum(b => (decimal?)b) ?? 0) +
                                      (thisMonthChargesList.Sum(c => (decimal?)c) ?? 0);

                var lastMonthInvoicesList = await _context.Invoices
                    .Where(i => i.PaidDate.HasValue && 
                               i.PaidDate.Value >= lastMonth && 
                               i.PaidDate.Value < thisMonth && 
                               i.Status == "Paid")
                    .Select(i => i.PaidAmount > 0 ? i.PaidAmount : i.TotalAmount)
                    .ToListAsync();
                
                var lastMonthPaidBookingsList = await _context.Bookings
                    .Where(b => b.UpdatedAt.HasValue && 
                               b.UpdatedAt.Value >= lastMonth && 
                               b.UpdatedAt.Value < thisMonth && 
                               b.Status == "Paid" &&
                               b.EstimatedTotalAmount.HasValue &&
                               b.EstimatedTotalAmount > 0)
                    .Select(b => b.EstimatedTotalAmount!.Value)
                    .ToListAsync();
                
                var lastMonthChargesList = await _context.Charges
                    .Where(c => c.ChargeDate >= lastMonth && c.ChargeDate < thisMonth)
                    .Select(c => c.TotalAmount)
                    .ToListAsync();
                
                var lastMonthRevenue = (lastMonthInvoicesList.Sum(i => (decimal?)i) ?? 0) +
                                      (lastMonthPaidBookingsList.Sum(b => (decimal?)b) ?? 0) +
                                      (lastMonthChargesList.Sum(c => (decimal?)c) ?? 0);

                var revenueGrowth = lastMonthRevenue > 0 ? 
                    ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100) : 0;

                var thisMonthBookings = await _context.Bookings
                    .Where(b => b.CreatedAt >= thisMonth && b.Status != "Cancelled")
                    .CountAsync();

                var lastMonthBookings = await _context.Bookings
                    .Where(b => b.CreatedAt >= lastMonth && b.CreatedAt < thisMonth && b.Status != "Cancelled")
                    .CountAsync();

                var bookingGrowth = lastMonthBookings > 0 ? 
                    ((double)(thisMonthBookings - lastMonthBookings) / lastMonthBookings * 100) : 0;

                return Ok(new
                {
                    today = new
                    {
                        bookings = todayBookings,
                        revenue = todayRevenue,
                        checkIns = todayCheckIns,
                        checkOuts = todayCheckOuts
                    },
                    occupancy = new
                    {
                        rate = Math.Round(occupancyRate, 1),
                        occupied = occupiedRooms,
                        total = totalRooms,
                        available = totalRooms - occupiedRooms
                    },
                    growth = new
                    {
                        revenue = Math.Round(revenueGrowth, 1),
                        bookings = Math.Round(bookingGrowth, 1)
                    },
                    totals = new
                    {
                        thisMonthRevenue,
                        thisMonthBookings,
                        lastMonthRevenue,
                        lastMonthBookings
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "L?i khi t?i th?ng kê dashboard", error = ex.Message });
            }
        }

        // GET: api/dashboard/revenue-trends
        [HttpGet("revenue-trends")]
        public async Task<ActionResult<object>> GetRevenueTrends([FromQuery] int days = 30)
        {
            try
            {
                var startDate = DateTime.Today.AddDays(-days);
                
                // Tính t?:
                // 1. Invoices dã thanh toán (PaidDate)
                // 2. Bookings dã thanh toán (Status = Paid, UpdatedAt)
                // 3. Charges (ChargeDate)
                var dailyInvoices = await _context.Invoices
                    .Where(i => i.PaidDate.HasValue && 
                               i.PaidDate.Value >= startDate && 
                               i.Status == "Paid")
                    .Select(i => new
                    {
                        date = i.PaidDate!.Value.Date,
                        amount = i.PaidAmount > 0 ? i.PaidAmount : i.TotalAmount
                    })
                    .ToListAsync();
                
                var dailyBookings = await _context.Bookings
                    .Where(b => b.UpdatedAt.HasValue && 
                               b.UpdatedAt.Value >= startDate && 
                               b.Status == "Paid" &&
                               b.EstimatedTotalAmount.HasValue &&
                               b.EstimatedTotalAmount > 0)
                    .Select(b => new
                    {
                        date = b.UpdatedAt!.Value.Date,
                        amount = b.EstimatedTotalAmount!.Value
                    })
                    .ToListAsync();
                
                var dailyCharges = await _context.Charges
                    .Where(c => c.ChargeDate >= startDate)
                    .Select(c => new
                    {
                        date = c.ChargeDate.Date,
                        amount = c.TotalAmount
                    })
                    .ToListAsync();
                
                // Combine all revenue sources
                var allDailyRevenue = dailyInvoices
                    .Select(i => new { i.date, i.amount })
                    .Concat(dailyBookings.Select(b => new { date = b.date, amount = b.amount }))
                    .Concat(dailyCharges.Select(c => new { date = c.date, amount = c.amount }))
                    .ToList();
                
                // Group by date manually
                var groupedRevenue = allDailyRevenue
                    .GroupBy(r => r.date)
                    .Select(g => new
                    {
                        date = g.Key,
                        revenue = g.Sum(r => (decimal?)r.amount) ?? 0,
                        invoiceCount = g.Count()
                    })
                    .OrderBy(x => x.date)
                    .ToList();

                // Fill missing dates with zero revenue
                var result = new List<object>();
                for (var date = startDate; date <= DateTime.Today; date = date.AddDays(1))
                {
                    var dayData = groupedRevenue.FirstOrDefault(d => d.date == date);
                    result.Add(new
                    {
                        date = date.ToString("yyyy-MM-dd"),
                        revenue = dayData?.revenue ?? 0,
                        invoiceCount = dayData?.invoiceCount ?? 0
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "L?i khi t?i xu hu?ng doanh thu", error = ex.Message });
            }
        }

        // GET: api/dashboard/occupancy-trends
        [HttpGet("occupancy-trends")]
        public async Task<ActionResult<object>> GetOccupancyTrends([FromQuery] int days = 30)
        {
            try
            {
                var startDate = DateTime.Today.AddDays(-days);
                var totalRooms = await _context.Rooms.CountAsync(r => r.IsAvailable);

                var result = new List<object>();
                
                for (var date = startDate; date <= DateTime.Today; date = date.AddDays(1))
                {
                    var occupiedRooms = await _context.Bookings
                        .Where(b => b.CheckInDate <= date && 
                                   b.CheckOutDate > date && 
                                   b.Status == "CheckedIn")
                        .CountAsync();

                    var occupancyRate = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0;

                    result.Add(new
                    {
                        date = date.ToString("yyyy-MM-dd"),
                        occupancyRate = Math.Round(occupancyRate, 1),
                        occupiedRooms,
                        totalRooms
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "L?i khi t?i xu hu?ng l?p d?y", error = ex.Message });
            }
        }

        // GET: api/dashboard/top-customers
        [HttpGet("top-customers")]
        public async Task<ActionResult<object>> GetTopCustomers([FromQuery] int limit = 10, [FromQuery] bool thisMonth = false)
        {
            try
            {
                var thisMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                
                // Tính toán TotalSpent t? Invoices dã thanh toán (Status = "Paid")
                // Load invoices tru?c, sau dó tính t?ng trên client side (SQLite không h? tr? Sum decimal)
                var invoiceQuery = _context.Invoices
                    .Where(i => i.Status == "Paid" && i.CustomerId > 0);
                
                if (thisMonth)
                {
                    // Ch? l?y invoices trong tháng này
                    invoiceQuery = invoiceQuery.Where(i => i.PaidDate.HasValue && 
                                                           i.PaidDate.Value >= thisMonthStart);
                }
                
                // Load invoices (không tính Sum ? dây vì SQLite không h? tr?)
                var invoices = await invoiceQuery
                    .Select(i => new
                    {
                        CustomerId = i.CustomerId,
                        Amount = i.PaidAmount > 0 ? i.PaidAmount : i.TotalAmount,
                        IssueDate = i.IssueDate
                    })
                    .ToListAsync();

                // Tính t?ng chi tiêu và s? lu?ng d?t phòng cho m?i customer trên client side
                var customerStats = invoices
                    .GroupBy(i => i.CustomerId)
                    .Select(g => new
                    {
                        CustomerId = g.Key,
                        TotalSpent = g.Sum(i => i.Amount),
                        BookingCount = g.Count(),
                        LastBookingDate = g.Max(i => i.IssueDate)
                    })
                    .ToList();

                // L?y thông tin customers t? Customer table
                var customerIds = customerStats.Select(s => s.CustomerId).ToList();
                var customers = await _context.Customers
                    .Where(c => customerIds.Contains(c.CustomerId))
                    .Select(c => new
                    {
                        customerId = c.CustomerId,
                        customerName = c.FullName,
                        email = c.Email,
                        loyaltyPoints = c.LoyaltyPoints
                    })
                    .ToListAsync();

                // Combine data và s?p x?p theo TotalSpent gi?m d?n
                var topCustomers = customers
                    .Join(customerStats,
                        c => c.customerId,
                        s => s.CustomerId,
                        (c, s) => new
                        {
                            customerId = c.customerId,
                            customerName = c.customerName,
                            email = c.email,
                            totalSpent = s.TotalSpent,
                            bookingCount = s.BookingCount,
                            loyaltyPoints = c.loyaltyPoints,
                            lastBookingDate = s.LastBookingDate
                        })
                    .OrderByDescending(c => c.totalSpent)
                    .Take(limit)
                    .ToList();

                return Ok(topCustomers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "L?i khi t?i top khách hàng", error = ex.Message });
            }
        }

        // GET: api/dashboard/recent-activities
        [HttpGet("recent-activities")]
        public async Task<ActionResult<object>> GetRecentActivities([FromQuery] int limit = 20)
        {
            try
            {
                var activities = await _context.AuditLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Take(limit)
                    .Select(a => new
                    {
                        a.LogId,
                        a.EntityName,
                        a.EntityId,
                        a.Action,
                        performedBy = a.PerformedBy ?? "System",
                        a.Timestamp,
                        description = GetActivityDescription(a.EntityName ?? "Unknown", a.Action ?? "Unknown", a.EntityId)
                    })
                    .ToListAsync();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "L?i khi t?i ho?t d?ng g?n dây", error = ex.Message });
            }
        }

        // GET: api/dashboard/room-status
        [HttpGet("room-status")]
        public async Task<ActionResult<object>> GetRoomStatus()
        {
            try
            {
                var roomStatuses = await _context.Rooms
                    .Include(r => r.RoomTypeNavigation)
                    .Where(r => r.IsAvailable)
                    .GroupBy(r => r.HousekeepingStatus)
                    .Select(g => new
                    {
                        status = g.Key,
                        count = g.Count(),
                        rooms = g.Select(r => new
                        {
                            r.RoomId,
                            r.RoomNumber,
                            roomType = r.RoomTypeNavigation != null ? r.RoomTypeNavigation.TypeName : null
                        }).ToList()
                    })
                    .ToListAsync();

                var totalRooms = await _context.Rooms.CountAsync(r => r.IsAvailable);

                return Ok(new
                {
                    totalRooms,
                    statusBreakdown = roomStatuses
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "L?i khi t?i tr?ng thái phòng", error = ex.Message });
            }
        }

        // GET: api/dashboard/service-revenue
        [HttpGet("service-revenue")]
        public async Task<ActionResult<object>> GetServiceRevenue([FromQuery] int days = 30)
        {
            try
            {
                var startDate = DateTime.Today.AddDays(-days);

                var serviceRevenue = await _context.Charges
                    .Include(c => c.Service)
                    .Where(c => c.ChargeDate >= startDate && c.Service != null)
                    .GroupBy(c => new { c.Service!.ServiceId, c.Service.ServiceName })
                    .Select(g => new
                    {
                        serviceId = g.Key.ServiceId,
                        serviceName = g.Key.ServiceName ?? "Unknown Service",
                        totalRevenue = g.Sum(c => c.Amount),
                        usageCount = g.Count(),
                        averageAmount = g.Average(c => c.Amount)
                    })
                    .OrderByDescending(x => x.totalRevenue)
                    .Take(10)
                    .ToListAsync();

                return Ok(serviceRevenue);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "L?i khi t?i doanh thu d?ch v?", error = ex.Message });
            }
        }

        private static string GetActivityDescription(string entityName, string action, int entityId)
        {
            return entityName switch
            {
                "Booking" => action switch
                {
                    "Create" => $"T?o d?t phòng m?i #{entityId}",
                    "Update" => $"C?p nh?t d?t phòng #{entityId}",
                    "Delete" => $"H?y d?t phòng #{entityId}",
                    _ => $"{action} d?t phòng #{entityId}"
                },
                "Customer" => action switch
                {
                    "Create" => $"Thêm khách hàng m?i #{entityId}",
                    "Update" => $"C?p nh?t thông tin khách hàng #{entityId}",
                    _ => $"{action} khách hàng #{entityId}"
                },
                "Room" => action switch
                {
                    "Create" => $"Thêm phòng m?i #{entityId}",
                    "Update" => $"C?p nh?t phòng #{entityId}",
                    _ => $"{action} phòng #{entityId}"
                },
                "Invoice" => action switch
                {
                    "Create" => $"T?o hóa don #{entityId}",
                    "Update" => $"C?p nh?t hóa don #{entityId}",
                    _ => $"{action} hóa don #{entityId}"
                },
                _ => $"{action} {entityName} #{entityId}"
            };
        }
    }
}

