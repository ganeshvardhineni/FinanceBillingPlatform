namespace FinanceBilling.Core.Exceptions;

public class PaymentExceedsBalanceException : Exception
{
    public decimal PaymentAmount { get; }
    public decimal BalanceDue { get; }

    public PaymentExceedsBalanceException(decimal paymentAmount, decimal balanceDue)
        : base($"Payment of {paymentAmount:C} exceeds balance due of {balanceDue:C}.")
    {
        PaymentAmount = paymentAmount;
        BalanceDue = balanceDue;
    }
}