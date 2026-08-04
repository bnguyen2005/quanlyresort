using QuanLyResort.Models;

namespace QuanLyResort.Services;

public interface IInvoiceService
{
    Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);
    Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber);
    Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
    Task<bool> ProcessPaymentAsync(int invoiceId, decimal amount, string paymentMethod, 
        string? paymentReference, string performedBy);
}

