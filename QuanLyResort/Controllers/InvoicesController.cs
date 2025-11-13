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
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ResortDbContext _context;
    private readonly IAuditService _auditService;

    public InvoicesController(IInvoiceService invoiceService, ResortDbContext context, IAuditService auditService)
    {
        _invoiceService = invoiceService;
        _context = context;
        _auditService = auditService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Accounting")]
    public async Task<IActionResult> GetAllInvoices([FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        // Reduced logging to avoid Railway rate limit (500 logs/sec)
        // const string logPrefix = "[InvoicesController.GetAllInvoices]";
        // var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        // Console.WriteLine($"{logPrefix} [{timestamp}] ========== START ==========");
        // Console.WriteLine($"{logPrefix} [{timestamp}] Request params: search={search}, status={status}, fromDate={fromDate}, toDate={toDate}");
        
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            // Console.WriteLine($"{logPrefix} [{timestamp}] User: {userEmail}, Role: {userRole}");
            
            var query = _context.Invoices
                .AsNoTracking()
                .Include(i => i.Booking)
                    .ThenInclude(b => b.Room)
                        .ThenInclude(r => r.RoomTypeNavigation)
                .Include(i => i.Customer)
                .AsQueryable();
            
            // Console.WriteLine($"{logPrefix} [{timestamp}] Initial query count: {await query.CountAsync()}");

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => i.InvoiceNumber.Contains(search) ||
                                          (i.Customer != null && i.Customer.FullName.Contains(search)));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(i => i.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(i => i.IssueDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i => i.IssueDate <= toDate.Value);
            }

            var invoices = await query.OrderByDescending(i => i.IssueDate).ToListAsync();
            
            // Console.WriteLine($"{logPrefix} [{timestamp}] ✅ Found {invoices.Count} invoices");
            // Console.WriteLine($"{logPrefix} [{timestamp}] ========== END (SUCCESS) ==========");
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            // Only log errors, not verbose debug info
            Console.WriteLine($"[InvoicesController.GetAllInvoices] ❌ Error: {ex.Message}");
            return StatusCode(500, new { message = "Failed to load invoices", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvoiceById(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null)
            return NotFound(new { message = "Invoice not found" });

        // Check authorization: customer can only view their own invoices
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var customerId = User.FindFirst("CustomerId")?.Value;

        if (userRole == "Customer" && customerId != invoice.CustomerId.ToString())
            return Forbid();

        return Ok(invoice);
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Manager,Accounting")]
    public async Task<IActionResult> GetInvoiceStatistics()
    {
        const string logPrefix = "[InvoicesController.GetInvoiceStatistics]";
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        Console.WriteLine($"{logPrefix} [{timestamp}] ========== START ==========");
        
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            Console.WriteLine($"{logPrefix} [{timestamp}] User: {userEmail}, Role: {userRole}");
            
            var totalInvoices = await _context.Invoices.CountAsync();
            Console.WriteLine($"{logPrefix} [{timestamp}] Total invoices: {totalInvoices}");
            
            var paidInvoices = await _context.Invoices.CountAsync(i => i.Status == "Paid");
            var pendingInvoices = await _context.Invoices.CountAsync(i => i.Status == "Issued" || i.Status == "PartiallyPaid");
            var cancelledInvoices = await _context.Invoices.CountAsync(i => i.Status == "Cancelled");
            
            Console.WriteLine($"{logPrefix} [{timestamp}] Paid: {paidInvoices}, Pending: {pendingInvoices}, Cancelled: {cancelledInvoices}");
            
            // SQLite không hỗ trợ SumAsync trên decimal trực tiếp -> chuyển sang client-side aggregation
            var paidInvoicesList = await _context.Invoices
                .Where(i => i.Status == "Paid")
                .Select(i => i.TotalAmount)
                .ToListAsync();
            var totalRevenue = paidInvoicesList.Sum(i => (decimal?)i) ?? 0;
            
            var pendingInvoicesList = await _context.Invoices
                .Where(i => i.Status == "Issued" || i.Status == "PartiallyPaid")
                .Select(i => i.BalanceDue)
                .ToListAsync();
            var totalPending = pendingInvoicesList.Sum(i => (decimal?)i) ?? 0;

            Console.WriteLine($"{logPrefix} [{timestamp}] Total revenue: {totalRevenue}, Total pending: {totalPending}");
            Console.WriteLine($"{logPrefix} [{timestamp}] ========== END (SUCCESS) ==========");

            return Ok(new
            {
                totalInvoices,
                paidInvoices,
                pendingInvoices,
                cancelledInvoices,
                totalRevenue,
                totalPending
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{logPrefix} [{timestamp}] ❌ ========== ERROR ==========");
            Console.WriteLine($"{logPrefix} [{timestamp}] ❌ Error message: {ex.Message}");
            Console.WriteLine($"{logPrefix} [{timestamp}] ❌ Stack trace: {ex.StackTrace}");
            Console.WriteLine($"{logPrefix} [{timestamp}] ❌ Inner exception: {ex.InnerException?.Message}");
            Console.WriteLine($"{logPrefix} [{timestamp}] ========== END (ERROR) ==========");
            return StatusCode(500, new { message = "Failed to load invoice statistics", error = ex.Message });
        }
    }

    [HttpPost("{id}/pay")]
    [Authorize(Roles = "Admin,Cashier,Accounting")]
    public async Task<IActionResult> ProcessPayment(int id, [FromBody] PaymentRequest request)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";
        var success = await _invoiceService.ProcessPaymentAsync(id, request.Amount, 
            request.PaymentMethod, request.PaymentReference, userEmail);

        if (!success)
            return NotFound(new { message = "Invoice not found" });

        return Ok(new { message = "Payment processed successfully" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelInvoice(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
            return NotFound(new { message = "Invoice not found" });

        var oldData = System.Text.Json.JsonSerializer.Serialize(invoice);

        invoice.Status = "Cancelled";
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Invoice", id, "Cancel", GetCurrentUsername(), oldData, System.Text.Json.JsonSerializer.Serialize(invoice));

        return Ok(new { message = "Invoice cancelled successfully.", invoice });
    }

    private string GetCurrentUsername()
    {
        return User.Identity?.Name ?? "Unknown";
    }
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PaymentReference { get; set; }
}

