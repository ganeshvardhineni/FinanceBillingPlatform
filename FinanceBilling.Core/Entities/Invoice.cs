using FinanceBilling.Core.Enums;

namespace FinanceBilling.Core.Entities;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; private set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public int CustomerId { get; set; }
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;

    public List<InvoiceItem> Items { get; private set; } = new();
    public List<Payment> Payments { get; private set; } = new();

    public static Invoice Create(string customerName, string customerEmail, DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));
        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("Customer email is required.", nameof(customerEmail));
        if (dueDate <= DateTime.UtcNow)
            throw new ArgumentException("Due date must be in the future.", nameof(dueDate));

        return new Invoice
        {
            InvoiceNumber = GenerateInvoiceNumber(),
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            IssueDate = DateTime.UtcNow,
            DueDate = dueDate,
            Status = InvoiceStatus.Draft
        };
    }

    public void AddItem(InvoiceItem item)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Items can only be added to draft invoices.");
        if (item == null) throw new ArgumentNullException(nameof(item));
        Items.Add(item);
        RecalculateTotals();
    }

    public void MarkAsSent()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be sent.");
        if (!Items.Any())
            throw new InvalidOperationException("Cannot send an invoice with no items.");

        Status = InvoiceStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyPayment(Payment payment)
    {
        if (payment == null) throw new ArgumentNullException(nameof(payment));
        if (Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Cannot apply payment to a cancelled invoice.");
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Invoice is already fully paid.");

        Payments.Add(payment);
        RecalculateTotals();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a paid invoice.");

        Status = InvoiceStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotals()
    {
        TotalAmount = Items.Sum(i => i.LineTotal);
        PaidAmount = Payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount);
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (BalanceDue <= 0)
            Status = InvoiceStatus.Paid;
        else if (PaidAmount > 0)
            Status = InvoiceStatus.PartiallyPaid;
        else if (DateTime.UtcNow > DueDate)
            Status = InvoiceStatus.Overdue;
    }

    private static string GenerateInvoiceNumber()
    {
        return $"INV-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
    }
}