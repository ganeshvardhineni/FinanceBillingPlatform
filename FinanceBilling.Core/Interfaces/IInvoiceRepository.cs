using FinanceBilling.Core.Entities;
using FinanceBilling.Core.Enums;

namespace FinanceBilling.Core.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status);
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
    Task<Invoice?> GetWithItemsAndPaymentsAsync(int id);
}