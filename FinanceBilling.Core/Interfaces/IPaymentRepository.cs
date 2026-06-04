using FinanceBilling.Core.Entities;

namespace FinanceBilling.Core.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IEnumerable<Payment>> GetByInvoiceIdAsync(int invoiceId);
}