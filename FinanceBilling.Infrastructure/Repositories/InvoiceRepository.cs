using FinanceBilling.Core.Entities;
using FinanceBilling.Core.Enums;
using FinanceBilling.Core.Interfaces;
using FinanceBilling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceBilling.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;

    public InvoiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(int id)
        => await _context.Invoices.FindAsync(id);

    public async Task<IEnumerable<Invoice>> GetAllAsync()
        => await _context.Invoices.Where(i => !i.IsDeleted).ToListAsync();

    public async Task AddAsync(Invoice entity)
    {
        await _context.Invoices.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Invoice entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Invoices.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var invoice = await GetByIdAsync(id);
        if (invoice != null)
        {
            invoice.IsDeleted = true;
            await UpdateAsync(invoice);
        }
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status)
        => await _context.Invoices
            .Where(i => i.Status == status && !i.IsDeleted)
            .ToListAsync();

    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        => await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Overdue && !i.IsDeleted)
            .ToListAsync();

    public async Task<Invoice?> GetWithItemsAndPaymentsAsync(int id)
        => await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
}