using FinanceBilling.Core.Enums;

namespace FinanceBilling.Core.Entities;

public class Payment : BaseEntity
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; private set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;

    public static Payment Create(int invoiceId, decimal amount, string paymentMethod)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("Payment method is required.", nameof(paymentMethod));

        return new Payment
        {
            InvoiceId = invoiceId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            PaymentDate = DateTime.UtcNow,
            Status = PaymentStatus.Pending
        };
    }

    public void Complete(string transactionReference)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be completed.");

        Status = PaymentStatus.Completed;
        TransactionReference = transactionReference;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be marked as failed.");

        Status = PaymentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}