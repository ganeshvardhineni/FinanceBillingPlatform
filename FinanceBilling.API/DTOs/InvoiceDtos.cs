using System.ComponentModel.DataAnnotations;

namespace FinanceBilling.API.DTOs;

public class CreateInvoiceDto
{
    [Required]
    [StringLength(150)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    public DateTime DueDate { get; set; }

    public string? Notes { get; set; }
}

public class AddInvoiceItemDto
{
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 9999)]
    public int Quantity { get; set; }

    [Range(0.01, 999999.99)]
    public decimal UnitPrice { get; set; }
}

public class ProcessPaymentDto
{
    [Range(0.01, 999999.99)]
    public decimal Amount { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = string.Empty;
}

public class InvoiceResponseDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}

public class InvoiceItemDto
{
    public int ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class PaymentDto
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
}