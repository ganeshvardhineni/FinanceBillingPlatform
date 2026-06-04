using FinanceBilling.Core.Entities;
using FinanceBilling.Core.Exceptions;
using FinanceBilling.Core.Interfaces;

namespace FinanceBilling.Core.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public InvoiceService(IInvoiceRepository invoiceRepository, IPaymentRepository paymentRepository)
    {
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
    }

    public async Task<Invoice> CreateInvoiceAsync(string customerName, string customerEmail, DateTime dueDate)
    {
        var invoice = Invoice.Create(customerName, customerEmail, dueDate);
        await _invoiceRepository.AddAsync(invoice);
        return invoice;
    }

    public async Task AddItemToInvoiceAsync(int invoiceId, string description, int quantity, decimal unitPrice)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId)
            ?? throw new InvoiceNotFoundException(invoiceId);

        var item = InvoiceItem.Create(description, quantity, unitPrice);
        invoice.AddItem(item);
        await _invoiceRepository.UpdateAsync(invoice);
    }

    public async Task SendInvoiceAsync(int invoiceId)
    {
        var invoice = await _invoiceRepository.GetWithItemsAndPaymentsAsync(invoiceId)
            ?? throw new InvoiceNotFoundException(invoiceId);

        invoice.MarkAsSent();
        await _invoiceRepository.UpdateAsync(invoice);
    }

    public async Task<Invoice> ProcessPaymentAsync(int invoiceId, decimal amount, string paymentMethod)
    {
        var invoice = await _invoiceRepository.GetWithItemsAndPaymentsAsync(invoiceId)
            ?? throw new InvoiceNotFoundException(invoiceId);

        if (amount > invoice.BalanceDue)
            throw new PaymentExceedsBalanceException(amount, invoice.BalanceDue);

        var payment = Payment.Create(invoiceId, amount, paymentMethod);
        payment.Complete(Guid.NewGuid().ToString());
        invoice.ApplyPayment(payment);

        await _paymentRepository.AddAsync(payment);
        await _invoiceRepository.UpdateAsync(invoice);

        return invoice;
    }

    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        => await _invoiceRepository.GetOverdueInvoicesAsync();

    public async Task<Invoice?> GetInvoiceDetailsAsync(int invoiceId)
        => await _invoiceRepository.GetWithItemsAndPaymentsAsync(invoiceId);
}