using FinanceBilling.Core.Entities;

namespace FinanceBilling.Core.Interfaces;

public interface IInvoiceService
{
    Task<Invoice> CreateInvoiceAsync(string customerName, string customerEmail, DateTime dueDate);
    Task AddItemToInvoiceAsync(int invoiceId, string description, int quantity, decimal unitPrice);
    Task SendInvoiceAsync(int invoiceId);
    Task<Invoice> ProcessPaymentAsync(int invoiceId, decimal amount, string paymentMethod);
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
    Task<Invoice?> GetInvoiceDetailsAsync(int invoiceId);
}