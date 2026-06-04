using FinanceBilling.Core.Entities;
using FinanceBilling.Core.Enums;
using FluentAssertions;

namespace FinanceBilling.Tests.Entities;

public class InvoiceTests
{
    // ─── Invoice.Create ───────────────────────────────────────────

    [Fact]
    public void Create_ValidInputs_ReturnsInvoiceWithDraftStatus()
    {
        // Arrange
        var customerName = "Acme Corp";
        var customerEmail = "billing@acme.com";
        var dueDate = DateTime.UtcNow.AddDays(30);

        // Act
        var invoice = Invoice.Create(customerName, customerEmail, dueDate);

        // Assert
        invoice.Should().NotBeNull();
        invoice.CustomerName.Should().Be(customerName);
        invoice.CustomerEmail.Should().Be(customerEmail);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.InvoiceNumber.Should().StartWith("INV-");
        invoice.Items.Should().BeEmpty();
        invoice.Payments.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "billing@acme.com")]
    [InlineData("  ", "billing@acme.com")]
    [InlineData(null, "billing@acme.com")]
    public void Create_EmptyCustomerName_ThrowsArgumentException(string? name, string email)
    {
        // Act
        var act = () => Invoice.Create(name!, email, DateTime.UtcNow.AddDays(30));

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Customer name*");
    }

    [Fact]
    public void Create_PastDueDate_ThrowsArgumentException()
    {
        // Act
        var act = () => Invoice.Create("Acme", "billing@acme.com", DateTime.UtcNow.AddDays(-1));

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Due date*");
    }

    // ─── Invoice.AddItem ──────────────────────────────────────────

    [Fact]
    public void AddItem_ToDraftInvoice_IncreasesTotalAmount()
    {
        // Arrange
        var invoice = CreateValidInvoice();
        var item = InvoiceItem.Create("Consulting", 2, 500m);

        // Act
        invoice.AddItem(item);

        // Assert
        invoice.Items.Should().HaveCount(1);
        invoice.TotalAmount.Should().Be(1000m);
    }

    [Fact]
    public void AddItem_ToSentInvoice_ThrowsInvalidOperationException()
    {
        // Arrange
        var invoice = CreateInvoiceWithItem();
        invoice.MarkAsSent();

        // Act
        var act = () => invoice.AddItem(InvoiceItem.Create("Extra", 1, 100m));

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*draft*");
    }

    [Fact]
    public void AddItem_NullItem_ThrowsArgumentNullException()
    {
        // Arrange
        var invoice = CreateValidInvoice();

        // Act
        var act = () => invoice.AddItem(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ─── Invoice.MarkAsSent ───────────────────────────────────────

    [Fact]
    public void MarkAsSent_DraftWithItems_StatusBecomeSent()
    {
        // Arrange
        var invoice = CreateInvoiceWithItem();

        // Act
        invoice.MarkAsSent();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Sent);
    }

    [Fact]
    public void MarkAsSent_DraftWithNoItems_ThrowsInvalidOperationException()
    {
        // Arrange
        var invoice = CreateValidInvoice();

        // Act
        var act = () => invoice.MarkAsSent();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*no items*");
    }

    [Fact]
    public void MarkAsSent_AlreadySent_ThrowsInvalidOperationException()
    {
        // Arrange
        var invoice = CreateInvoiceWithItem();
        invoice.MarkAsSent();

        // Act
        var act = () => invoice.MarkAsSent();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    // ─── Invoice.ApplyPayment ─────────────────────────────────────

    [Fact]
    public void ApplyPayment_FullAmount_StatusBecomePaid()
    {
        // Arrange
        var invoice = CreateInvoiceWithItem(); // total = 1000
        invoice.MarkAsSent();
        var payment = Payment.Create(invoice.Id, 1000m, "Bank Transfer");
        payment.Complete("TXN-001");

        // Act
        invoice.ApplyPayment(payment);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.BalanceDue.Should().Be(0);
    }

    [Fact]
    public void ApplyPayment_PartialAmount_StatusBecomesPartiallyPaid()
    {
        // Arrange
        var invoice = CreateInvoiceWithItem(); // total = 1000
        invoice.MarkAsSent();
        var payment = Payment.Create(invoice.Id, 400m, "UPI");
        payment.Complete("TXN-002");

        // Act
        invoice.ApplyPayment(payment);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
        invoice.BalanceDue.Should().Be(600m);
    }

    [Fact]
    public void ApplyPayment_ToCancelledInvoice_ThrowsInvalidOperationException()
    {
        // Arrange
        var invoice = CreateInvoiceWithItem();
        invoice.Cancel();
        var payment = Payment.Create(invoice.Id, 500m, "Cash");
        payment.Complete("TXN-003");

        // Act
        var act = () => invoice.ApplyPayment(payment);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*cancelled*");
    }

    [Fact]
    public void ApplyPayment_ToAlreadyPaidInvoice_ThrowsInvalidOperationException()
    {
        // Arrange
        var invoice = CreateInvoiceWithItem();
        invoice.MarkAsSent();
        var p1 = Payment.Create(invoice.Id, 1000m, "Card");
        p1.Complete("TXN-004");
        invoice.ApplyPayment(p1);

        var p2 = Payment.Create(invoice.Id, 100m, "Card");
        p2.Complete("TXN-005");

        // Act
        var act = () => invoice.ApplyPayment(p2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*already fully paid*");
    }

    // ─── Invoice.Cancel ───────────────────────────────────────────

    [Fact]
    public void Cancel_UnpaidInvoice_StatusBecomesCancelled()
    {
        // Arrange
        var invoice = CreateInvoiceWithItem();
        invoice.MarkAsSent();

        // Act
        invoice.Cancel();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void Cancel_PaidInvoice_ThrowsInvalidOperationException()
    {
        // Arrange
        var invoice = CreateInvoiceWithItem();
        invoice.MarkAsSent();
        var payment = Payment.Create(invoice.Id, 1000m, "NEFT");
        payment.Complete("TXN-006");
        invoice.ApplyPayment(payment);

        // Act
        var act = () => invoice.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*paid invoice*");
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private static Invoice CreateValidInvoice()
        => Invoice.Create("Acme Corp", "billing@acme.com", DateTime.UtcNow.AddDays(30));

    private static Invoice CreateInvoiceWithItem()
    {
        var invoice = CreateValidInvoice();
        invoice.AddItem(InvoiceItem.Create("Consulting", 2, 500m));
        return invoice;
    }
}