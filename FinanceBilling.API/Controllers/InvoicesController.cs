using FinanceBilling.API.DTOs;
using FinanceBilling.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoicesController(IInvoiceService invoiceService, IInvoiceRepository invoiceRepository)
    {
        _invoiceService = invoiceService;
        _invoiceRepository = invoiceRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Viewer")]
    public async Task<IActionResult> GetAll()
    {
        var invoices = await _invoiceRepository.GetAllAsync();
        var result = invoices.Select(i => new InvoiceResponseDto
        {
            InvoiceId = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            CustomerName = i.CustomerName,
            CustomerEmail = i.CustomerEmail,
            TotalAmount = i.TotalAmount,
            PaidAmount = i.PaidAmount,
            BalanceDue = i.BalanceDue,
            Status = i.Status.ToString(),
            DueDate = i.DueDate
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,Viewer")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _invoiceService.GetInvoiceDetailsAsync(id);
        if (invoice == null) return NotFound(new { error = $"Invoice {id} not found." });

        var result = new InvoiceResponseDto
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerName = invoice.CustomerName,
            CustomerEmail = invoice.CustomerEmail,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            BalanceDue = invoice.BalanceDue,
            Status = invoice.Status.ToString(),
            DueDate = invoice.DueDate,
            Items = invoice.Items.Select(item => new InvoiceItemDto
            {
                ItemId = item.Id,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            }).ToList(),
            Payments = invoice.Payments.Select(p => new PaymentDto
            {
                PaymentId = p.Id,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                TransactionReference = p.TransactionReference,
                Status = p.Status.ToString(),
                PaymentDate = p.PaymentDate
            }).ToList()
        };

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var invoice = await _invoiceService.CreateInvoiceAsync(
            dto.CustomerName,
            dto.CustomerEmail,
            dto.DueDate);

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, new
        {
            invoiceId = invoice.Id,
            invoiceNumber = invoice.InvoiceNumber,
            message = "Invoice created successfully."
        });
    }

    [HttpPost("{id}/items")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddItem(int id, [FromBody] AddInvoiceItemDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _invoiceService.AddItemToInvoiceAsync(id, dto.Description, dto.Quantity, dto.UnitPrice);
        return Ok(new { message = "Item added successfully." });
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Send(int id)
    {
        await _invoiceService.SendInvoiceAsync(id);
        return Ok(new { message = "Invoice sent successfully." });
    }

    [HttpPost("{id}/payments")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ProcessPayment(int id, [FromBody] ProcessPaymentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var invoice = await _invoiceService.ProcessPaymentAsync(id, dto.Amount, dto.PaymentMethod);

        return Ok(new
        {
            message = "Payment processed successfully.",
            balanceDue = invoice.BalanceDue,
            status = invoice.Status.ToString()
        });
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,Manager,Viewer")]
    public async Task<IActionResult> GetOverdue()
    {
        var invoices = await _invoiceService.GetOverdueInvoicesAsync();
        return Ok(invoices.Select(i => new InvoiceResponseDto
        {
            InvoiceId = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            CustomerName = i.CustomerName,
            TotalAmount = i.TotalAmount,
            BalanceDue = i.BalanceDue,
            Status = i.Status.ToString(),
            DueDate = i.DueDate
        }));
    }
}