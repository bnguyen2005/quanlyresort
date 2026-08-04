using Microsoft.EntityFrameworkCore;
using QuanLyResort.Data;
using QuanLyResort.Models;
using QuanLyResort.Repositories;

namespace QuanLyResort.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ResortDbContext _context;

    public InvoiceService(IUnitOfWork unitOfWork, IAuditService auditService, ResortDbContext context)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _context = context;
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
    {
        return await _context.Invoices
            .Include(i => i.Booking)
                .ThenInclude(b => b.Room)
                    .ThenInclude(r => r.RoomTypeNavigation)
            .Include(i => i.Booking)
                .ThenInclude(b => b.Customer)
            .Include(i => i.Customer)
            .Include(i => i.Charges)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
    }

    public async Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber)
    {
        return await _context.Invoices
            .Include(i => i.Booking)
            .Include(i => i.Customer)
            .Include(i => i.Charges)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
    {
        return await _context.Invoices
            .Include(i => i.Booking)
            .Include(i => i.Customer)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
    }

    public async Task<bool> ProcessPaymentAsync(int invoiceId, decimal amount, string paymentMethod,
        string? paymentReference, string performedBy)
    {
        var invoice = await GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
            return false;

        var oldStatus = invoice.Status;
        invoice.PaidAmount += amount;
        invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;

        if (invoice.BalanceDue <= 0)
        {
            invoice.Status = "Paid";
            invoice.PaidDate = DateTime.UtcNow;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = "PartiallyPaid";
        }

        invoice.PaymentMethod = paymentMethod;
        invoice.PaymentReference = paymentReference;
        invoice.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Invoices.Update(invoice);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Invoice", invoiceId, "Payment", performedBy,
            $"Status: {oldStatus}, Paid: {invoice.PaidAmount - amount}",
            $"Status: {invoice.Status}, Paid: {invoice.PaidAmount}",
            $"Payment of {amount:C} received via {paymentMethod}");

        return true;
    }
}

