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
        private readonly ResortDbContext _context;

        public DashboardController(ResortDbContext context)
        {
            _context = context;
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

                // Today's bookings - đặt phòng được tạo hôm nay (không phải check-in hôm nay)
                var todayBookings = await _context.Bookings
                    .Where(b => b.CreatedAt.Date == today && b.Status != "Cancelled")
                    .CountAsync();

                // Today's revenue - tính từ:
                // 1. Invoices đã thanh toán hôm nay (PaidDate = today)
                // 2. Bookings đã thanh toán hôm nay (PaymentStatus = Paid và UpdatedAt = today)
                var todayInvoicesList = await _context.Invoices
                    .Where(i => i.PaidDate.HasValue && 
                               i.PaidDate.Value.Date == today && 
                               i.Status == "Paid")
                    .Select(i => i.PaidAmount > 0 ? i.PaidAmount : i.TotalAmount)
                    .ToListAsync();
                
                // Cũng tính từ Bookings đã thanh toán hôm nay (nếu có PaymentStatus)
                var todayPaidBookingsList = await _context.Bookings
                    .Where(b => b.UpdatedAt.Date == today && 
                               (b.Status == "Confirmed" || b.Status == "CheckedIn") &&
                               b.EstimatedTotalAmount > 0)
                    .Select(b => b.EstimatedTotalAmount)
                    .ToListAsync();
                
                var todayRevenue = (todayInvoicesList.Sum(i => (decimal?)i) ?? 0) + 
                                  (todayPaidBookingsList.Sum(b => (decimal?)b) ?? 0);

                // Today's check-ins - bookings có CheckInDate = today và đã check-in
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

                // This month vs last month
                // SQLite không hỗ trợ SumAsync trên decimal trực tiếp -> chuyển sang client-side aggregation
                var thisMonthInvoicesList = await _context.Invoices
                    .Where(i => i.IssueDate >= thisMonth && i.Status == "Paid")
                    .Select(i => i.TotalAmount)
                    .ToListAsync();
                var thisMonthRevenue = thisMonthInvoicesList.Sum(i => (decimal?)i) ?? 0;

                var lastMonthInvoicesList = await _context.Invoices
                    .Where(i => i.IssueDate >= lastMonth && i.IssueDate < thisMonth && i.Status == "Paid")
                    .Select(i => i.TotalAmount)
                    .ToListAsync();
                var lastMonthRevenue = lastMonthInvoicesList.Sum(i => (decimal?)i) ?? 0;

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
                return StatusCode(500, new { message = "Lỗi khi tải thống kê dashboard", error = ex.Message });
            }
        }

        // GET: api/dashboard/revenue-trends
        [HttpGet("revenue-trends")]
        public async Task<ActionResult<object>> GetRevenueTrends([FromQuery] int days = 30)
        {
            try
            {
                var startDate = DateTime.Today.AddDays(-days);
                
                var dailyRevenue = await _context.Invoices
                    .Where(i => i.IssueDate >= startDate && i.Status == "Paid")
                    .GroupBy(i => i.IssueDate.Date)
                    .Select(g => new
                    {
                        date = g.Key,
                        revenue = g.Sum(i => i.TotalAmount),
                        invoiceCount = g.Count()
                    })
                    .OrderBy(x => x.date)
                    .ToListAsync();

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
                return StatusCode(500, new { message = "Lỗi khi tải xu hướng doanh thu", error = ex.Message });
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
                return StatusCode(500, new { message = "Lỗi khi tải xu hướng lấp đầy", error = ex.Message });
            }
        }

        // GET: api/dashboard/top-customers
        [HttpGet("top-customers")]
        public async Task<ActionResult<object>> GetTopCustomers([FromQuery] int limit = 10)
        {
            try
            {
                var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                var topCustomers = await _context.Invoices
                    .Include(i => i.Customer)
                    .Where(i => i.IssueDate >= thisMonth && i.Status == "Paid")
                    .GroupBy(i => new { i.CustomerId, i.Customer.FullName, i.Customer.Email })
                    .Select(g => new
                    {
                        customerId = g.Key.CustomerId,
                        customerName = g.Key.FullName,
                        email = g.Key.Email,
                        totalSpent = g.Sum(i => i.TotalAmount),
                        bookingCount = g.Count(),
                        averageSpent = g.Average(i => i.TotalAmount)
                    })
                    .OrderByDescending(x => x.totalSpent)
                    .Take(limit)
                    .ToListAsync();

                return Ok(topCustomers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải top khách hàng", error = ex.Message });
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
                        a.PerformedBy,
                        a.Timestamp,
                        description = GetActivityDescription(a.EntityName, a.Action, a.EntityId)
                    })
                    .ToListAsync();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải hoạt động gần đây", error = ex.Message });
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
                return StatusCode(500, new { message = "Lỗi khi tải trạng thái phòng", error = ex.Message });
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
                    .GroupBy(c => new { c.Service.ServiceId, c.Service.ServiceName })
                    .Select(g => new
                    {
                        serviceId = g.Key.ServiceId,
                        serviceName = g.Key.ServiceName,
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
                return StatusCode(500, new { message = "Lỗi khi tải doanh thu dịch vụ", error = ex.Message });
            }
        }

        private static string GetActivityDescription(string entityName, string action, int entityId)
        {
            return entityName switch
            {
                "Booking" => action switch
                {
                    "Create" => $"Tạo đặt phòng mới #{entityId}",
                    "Update" => $"Cập nhật đặt phòng #{entityId}",
                    "Delete" => $"Hủy đặt phòng #{entityId}",
                    _ => $"{action} đặt phòng #{entityId}"
                },
                "Customer" => action switch
                {
                    "Create" => $"Thêm khách hàng mới #{entityId}",
                    "Update" => $"Cập nhật thông tin khách hàng #{entityId}",
                    _ => $"{action} khách hàng #{entityId}"
                },
                "Room" => action switch
                {
                    "Create" => $"Thêm phòng mới #{entityId}",
                    "Update" => $"Cập nhật phòng #{entityId}",
                    _ => $"{action} phòng #{entityId}"
                },
                "Invoice" => action switch
                {
                    "Create" => $"Tạo hóa đơn #{entityId}",
                    "Update" => $"Cập nhật hóa đơn #{entityId}",
                    _ => $"{action} hóa đơn #{entityId}"
                },
                _ => $"{action} {entityName} #{entityId}"
            };
        }
    }
}
