using FinanceBilling.Core.Entities;
using FinanceBilling.Core.Enums;
using FluentAssertions;

namespace FinanceBilling.Tests.Entities;

public class PaymentTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsPendingPayment()
    {
        // Act
        var payment = Payment.Create(1, 500m, "UPI");

        // Assert
        payment.Should().NotBeNull();
        payment.Amount.Should().Be(500m);
        payment.PaymentMethod.Should().Be("UPI");
        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Create_ZeroAmount_ThrowsArgumentException()
    {
        // Act
        var act = () => Payment.Create(1, 0m, "UPI");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Create_NegativeAmount_ThrowsArgumentException()
    {
        // Act
        var act = () => Payment.Create(1, -100m, "UPI");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyPaymentMethod_ThrowsArgumentException()
    {
        // Act
        var act = () => Payment.Create(1, 500m, "");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Payment method*");
    }

    [Fact]
    public void Complete_PendingPayment_StatusBecomesCompleted()
    {
        // Arrange
        var payment = Payment.Create(1, 500m, "NEFT");

        // Act
        payment.Complete("TXN-123");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.TransactionReference.Should().Be("TXN-123");
    }

    [Fact]
    public void Complete_AlreadyCompleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var payment = Payment.Create(1, 500m, "NEFT");
        payment.Complete("TXN-123");

        // Act
        var act = () => payment.Complete("TXN-456");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Fail_PendingPayment_StatusBecomesFailed()
    {
        // Arrange
        var payment = Payment.Create(1, 500m, "Card");

        // Act
        payment.Fail();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void Fail_CompletedPayment_ThrowsInvalidOperationException()
    {
        // Arrange
        var payment = Payment.Create(1, 500m, "Card");
        payment.Complete("TXN-789");

        // Act
        var act = () => payment.Fail();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}