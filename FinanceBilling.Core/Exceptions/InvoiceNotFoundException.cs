namespace FinanceBilling.Core.Exceptions;

public class InvoiceNotFoundException : Exception
{
    public int InvoiceId { get; }

    public InvoiceNotFoundException(int invoiceId)
        : base($"Invoice with ID {invoiceId} was not found.")
    {
        InvoiceId = invoiceId;
    }
}