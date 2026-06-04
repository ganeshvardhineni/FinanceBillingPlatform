using FinanceBilling.Core.Entities;
using FinanceBilling.Core.Interfaces;
using FinanceBilling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceBilling.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(int id)
        => await _context.Payments.FindAsync(id);

    public async Task<IEnumerable<Payment>> GetAllAsync()
        => await _context.Payments.Where(p => !p.IsDeleted).ToListAsync();

    public async Task AddAsync(Payment entity)
    {
        await _context.Payments.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Payments.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var payment = await GetByIdAsync(id);
        if (payment != null)
        {
            payment.IsDeleted = true;
            await UpdateAsync(payment);
        }
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(int invoiceId)
        => await _context.Payments
            .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted)
            .ToListAsync();
}