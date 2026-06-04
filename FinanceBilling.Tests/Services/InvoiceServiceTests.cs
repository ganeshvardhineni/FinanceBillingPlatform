using FinanceBilling.Core.Entities;
using FinanceBilling.Core.Exceptions;
using FinanceBilling.Core.Interfaces;
using FinanceBilling.Core.Services;
using FluentAssertions;
using Moq;

namespace FinanceBilling.Tests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _invoiceRepoMock = new Mock<IInvoiceRepository>();
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _service = new InvoiceService(_invoiceRepoMock.Object, _paymentRepoMock.Object);
    }

    // ─── CreateInvoiceAsync ───────────────────────────────────────

    [Fact]
    public async Task CreateInvoiceAsync_ValidInputs_AddsAndReturnsInvoice()
    {
        // Arrange
        _invoiceRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Invoice>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateInvoiceAsync(
            "Acme Corp", "billing@acme.com", DateTime.UtcNow.AddDays(30));

        // Assert
        result.Should().NotBeNull();
        result.CustomerName.Should().Be("Acme Corp");
        _invoiceRepoMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
    }

    // ─── AddItemToInvoiceAsync ────────────────────────────────────

    [Fact]
    public async Task AddItemToInvoiceAsync_InvoiceNotFound_ThrowsInvoiceNotFoundException()
    {
        // Arrange
        _invoiceRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Invoice?)null);

        // Act
        var act = async () => await _service.AddItemToInvoiceAsync(99, "Service", 1, 100m);

        // Assert
        await act.Should().ThrowAsync<InvoiceNotFoundException>()
            .WithMessage("*99*");
    }

    [Fact]
    public async Task AddItemToInvoiceAsync_ValidInvoice_UpdatesInvoice()
    {
        // Arrange
        var invoice = Invoice.Create("Acme", "billing@acme.com", DateTime.UtcNow.AddDays(30));

        _invoiceRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(invoice);
        _invoiceRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Invoice>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AddItemToInvoiceAsync(1, "Consulting", 3, 200m);

        // Assert
        invoice.Items.Should().HaveCount(1);
        invoice.TotalAmount.Should().Be(600m);
        _invoiceRepoMock.Verify(r => r.UpdateAsync(invoice), Times.Once);
    }

    // ─── SendInvoiceAsync ─────────────────────────────────────────

    [Fact]
    public async Task SendInvoiceAsync_InvoiceNotFound_ThrowsInvoiceNotFoundException()
    {
        // Arrange
        _invoiceRepoMock
            .Setup(r => r.GetWithItemsAndPaymentsAsync(It.IsAny<int>()))
            .ReturnsAsync((Invoice?)null);

        // Act
        var act = async () => await _service.SendInvoiceAsync(99);

        // Assert
        await act.Should().ThrowAsync<InvoiceNotFoundException>();
    }

    [Fact]
    public async Task SendInvoiceAsync_ValidInvoice_MarksAsSent()
    {
        // Arrange
        var invoice = Invoice.Create("Acme", "billing@acme.com", DateTime.UtcNow.AddDays(30));
        invoice.AddItem(InvoiceItem.Create("Service", 1, 500m));

        _invoiceRepoMock
            .Setup(r => r.GetWithItemsAndPaymentsAsync(1))
            .ReturnsAsync(invoice);
        _invoiceRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Invoice>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SendInvoiceAsync(1);

        // Assert
        _invoiceRepoMock.Verify(r => r.UpdateAsync(invoice), Times.Once);
    }

    // ─── ProcessPaymentAsync ──────────────────────────────────────

    [Fact]
    public async Task ProcessPaymentAsync_AmountExceedsBalance_ThrowsPaymentExceedsBalanceException()
    {
        // Arrange
        var invoice = Invoice.Create("Acme", "billing@acme.com", DateTime.UtcNow.AddDays(30));
        invoice.AddItem(InvoiceItem.Create("Service", 1, 500m));
        invoice.MarkAsSent();

        _invoiceRepoMock
            .Setup(r => r.GetWithItemsAndPaymentsAsync(1))
            .ReturnsAsync(invoice);

        // Act
        var act = async () => await _service.ProcessPaymentAsync(1, 9999m, "UPI");

        // Assert
        await act.Should().ThrowAsync<PaymentExceedsBalanceException>()
            .WithMessage("*exceeds balance*");
    }

    [Fact]
    public async Task ProcessPaymentAsync_ValidPayment_ReturnsUpdatedInvoice()
    {
        // Arrange
        var invoice = Invoice.Create("Acme", "billing@acme.com", DateTime.UtcNow.AddDays(30));
        invoice.AddItem(InvoiceItem.Create("Service", 1, 500m));
        invoice.MarkAsSent();

        _invoiceRepoMock
            .Setup(r => r.GetWithItemsAndPaymentsAsync(1))
            .ReturnsAsync(invoice);
        _invoiceRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Invoice>()))
            .Returns(Task.CompletedTask);
        _paymentRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ProcessPaymentAsync(1, 500m, "NEFT");

        // Assert
        result.BalanceDue.Should().Be(0);
        _paymentRepoMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
        _invoiceRepoMock.Verify(r => r.UpdateAsync(invoice), Times.Once);
    }

    // ─── Constructor Guard ────────────────────────────────────────

    [Fact]
    public void Constructor_NullInvoiceRepo_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new InvoiceService(null!, _paymentRepoMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullPaymentRepo_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new InvoiceService(_invoiceRepoMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}