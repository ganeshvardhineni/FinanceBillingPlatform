using System.ComponentModel.DataAnnotations;

namespace FinanceBilling.Web.ViewModels;

public class CreateInvoiceViewModel
{
    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    [Display(Name = "Customer Email")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Due date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);

    public string? Notes { get; set; }
}

public class AddItemViewModel
{
    public int InvoiceId { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, 9999, ErrorMessage = "Quantity must be between 1 and 9999")]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, 999999.99, ErrorMessage = "Unit price must be greater than zero")]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }
}

public class ProcessPaymentViewModel
{
    public int InvoiceId { get; set; }

    [Required]
    [Range(0.01, 999999.99, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Payment method is required")]
    [Display(Name = "Payment Method")]
    public string PaymentMethod { get; set; } = string.Empty;
}