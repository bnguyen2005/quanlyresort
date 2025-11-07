using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/audit-log")]
[Authorize(Roles = "Admin,Manager,Accounting")]
public class AuditController : ControllerBase
{
    private readonly ResortDbContext _context;

    public AuditController(ResortDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách audit logs với filter
    /// GET /api/audit-log hoặc /api/audit-log/logs
    /// </summary>
    [HttpGet]
    [HttpGet("logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityName = null,
        [FromQuery] int? entityId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? performedBy = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (entityId.HasValue)
            query = query.Where(a => a.EntityId == entityId);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrEmpty(performedBy))
            query = query.Where(a => a.PerformedBy != null && a.PerformedBy.Contains(performedBy));

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp < endDate.Value.AddDays(1));

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            logs,
            pagination = new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    /// <summary>
    /// Lấy audit logs theo entity cụ thể
    /// </summary>
    [HttpGet("entity/{entityName}/{entityId}")]
    public async Task<IActionResult> GetEntityAuditLogs(string entityName, int entityId)
    {
        var logs = await _context.AuditLogs
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        return Ok(logs);
    }

    /// <summary>
    /// Lấy thống kê hoạt động theo user
    /// </summary>
    [HttpGet("user-activity")]
    public async Task<IActionResult> GetUserActivity(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp < endDate.Value.AddDays(1));

        var userActivity = await query
            .Where(a => a.PerformedBy != null)
            .GroupBy(a => a.PerformedBy)
            .Select(g => new
            {
                user = g.Key,
                totalActions = g.Count(),
                lastActivity = g.Max(a => a.Timestamp),
                actionsByType = g.GroupBy(a => a.Action)
                    .Select(ag => new { action = ag.Key, count = ag.Count() })
                    .ToList()
            })
            .OrderByDescending(x => x.totalActions)
            .ToListAsync();

        return Ok(userActivity);
    }

    /// <summary>
    /// Lấy thống kê theo entity
    /// </summary>
    [HttpGet("entity-statistics")]
    public async Task<IActionResult> GetEntityStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp < endDate.Value.AddDays(1));

        var entityStats = await query
            .GroupBy(a => a.EntityName)
            .Select(g => new
            {
                entityName = g.Key,
                totalActions = g.Count(),
                creates = g.Count(a => a.Action == "Create"),
                updates = g.Count(a => a.Action == "Update"),
                deletes = g.Count(a => a.Action == "Delete"),
                lastActivity = g.Max(a => a.Timestamp)
            })
            .OrderByDescending(x => x.totalActions)
            .ToListAsync();

        return Ok(entityStats);
    }

    /// <summary>
    /// Lấy danh sách action types
    /// </summary>
    [HttpGet("action-types")]
    public async Task<IActionResult> GetActionTypes()
    {
        var actions = await _context.AuditLogs
            .Select(a => a.Action)
            .Distinct()
            .OrderBy(a => a)
            .ToListAsync();

        return Ok(actions);
    }

    /// <summary>
    /// Lấy danh sách entity types
    /// </summary>
    [HttpGet("entity-types")]
    public async Task<IActionResult> GetEntityTypes()
    {
        var entities = await _context.AuditLogs
            .Select(a => a.EntityName)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync();

        return Ok(entities);
    }

    /// <summary>
    /// Xóa logs cũ (Admin only)
    /// </summary>
    [HttpDelete("cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CleanupOldLogs([FromQuery] int daysToKeep = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        var oldLogs = await _context.AuditLogs
            .Where(a => a.Timestamp < cutoffDate)
            .ToListAsync();

        _context.AuditLogs.RemoveRange(oldLogs);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = $"Cleaned up {oldLogs.Count} logs older than {daysToKeep} days",
            deletedCount = oldLogs.Count
        });
    }

    [HttpGet("daily-reconciliation")]
    public async Task<IActionResult> GetDailyReconciliation([FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.Today;
        var startOfDay = targetDate.Date;
        var endOfDay = startOfDay.AddDays(1);

        // Get all bookings that were active during the day
        var activeBookings = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.Customer)
            .Where(b => b.CheckInDate <= endOfDay && b.CheckOutDate > startOfDay)
            .ToListAsync();

        // Get all charges for the day
        var dailyCharges = await _context.Charges
            .Where(c => c.ChargeDate >= startOfDay && c.ChargeDate < endOfDay)
            .ToListAsync();

        // Get all invoices issued during the day
        var dailyInvoices = await _context.Invoices
            .Include(i => i.Booking)
            .Include(i => i.Customer)
            .Where(i => i.IssueDate >= startOfDay && i.IssueDate < endOfDay)
            .ToListAsync();

        // Get payments received
        var dailyPayments = await _context.Invoices
            .Where(i => i.PaidDate.HasValue && i.PaidDate.Value >= startOfDay && i.PaidDate.Value < endOfDay)
            .ToListAsync();

        var totalCharges = dailyCharges.Sum(c => c.TotalAmount);
        var totalInvoices = dailyInvoices.Sum(i => i.TotalAmount);
        var totalPayments = dailyPayments.Sum(i => i.PaidAmount);

        // Check for mismatches
        var mismatches = new List<string>();
        
        foreach (var invoice in dailyInvoices)
        {
            var invoiceCharges = dailyCharges.Where(c => c.BookingId == invoice.BookingId).Sum(c => c.TotalAmount);
            if (Math.Abs(invoice.SubTotal - invoiceCharges) > 0.01m)
            {
                mismatches.Add($"Invoice {invoice.InvoiceNumber}: Charges mismatch (Invoice: {invoice.SubTotal}, Charges: {invoiceCharges})");
            }
        }

        return Ok(new
        {
            date = targetDate,
            summary = new
            {
                activeBookings = activeBookings.Count,
                checkedIn = activeBookings.Count(b => b.Status == "CheckedIn"),
                checkedOut = activeBookings.Count(b => b.Status == "CheckedOut" && 
                    b.ActualCheckOutTime >= startOfDay && b.ActualCheckOutTime < endOfDay)
            },
            financial = new
            {
                totalCharges,
                totalInvoices,
                totalPayments,
                outstandingBalance = totalInvoices - totalPayments
            },
            details = new
            {
                bookings = activeBookings.Select(b => new
                {
                    b.BookingCode,
                    b.Status,
                    customerName = b.Customer.FullName,
                    roomNumber = b.Room?.RoomNumber,
                    b.CheckInDate,
                    b.CheckOutDate
                }),
                invoices = dailyInvoices.Select(i => new
                {
                    i.InvoiceNumber,
                    i.Status,
                    i.TotalAmount,
                    i.PaidAmount,
                    i.BalanceDue
                })
            },
            mismatches,
            reconciliationStatus = mismatches.Count == 0 ? "Clean" : "HasMismatches"
        });
    }
}

