using FinanceBilling.Core.Exceptions;
using FinanceBilling.Core.Interfaces;
using FinanceBilling.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinanceBilling.Web.Controllers;

public class InvoiceController : Controller
{
    private readonly IInvoiceService _invoiceService;
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoiceController(IInvoiceService invoiceService, IInvoiceRepository invoiceRepository)
    {
        _invoiceService = invoiceService;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IActionResult> Index()
    {
        var invoices = await _invoiceRepository.GetAllAsync();
        return View(invoices);
    }

    public IActionResult Create()
    {
        return View(new CreateInvoiceViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInvoiceViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var invoice = await _invoiceService.CreateInvoiceAsync(
                model.CustomerName,
                model.CustomerEmail,
                model.DueDate);

            TempData["Success"] = $"Invoice {invoice.InvoiceNumber} created successfully.";
            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _invoiceService.GetInvoiceDetailsAsync(id);
        if (invoice == null)
            return NotFound();

        return View(invoice);
    }

    public async Task<IActionResult> AddItem(int id)
    {
        var invoice = await _invoiceService.GetInvoiceDetailsAsync(id);
        if (invoice == null) return NotFound();

        return View(new AddItemViewModel { InvoiceId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(AddItemViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _invoiceService.AddItemToInvoiceAsync(
                model.InvoiceId,
                model.Description,
                model.Quantity,
                model.UnitPrice);

            TempData["Success"] = "Item added successfully.";
            return RedirectToAction(nameof(Details), new { id = model.InvoiceId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id)
    {
        try
        {
            await _invoiceService.SendInvoiceAsync(id);
            TempData["Success"] = "Invoice sent successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    public IActionResult ProcessPayment(int id)
    {
        return View(new ProcessPaymentViewModel { InvoiceId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(ProcessPaymentViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _invoiceService.ProcessPaymentAsync(
                model.InvoiceId,
                model.Amount,
                model.PaymentMethod);

            TempData["Success"] = "Payment processed successfully.";
            return RedirectToAction(nameof(Details), new { id = model.InvoiceId });
        }
        catch (PaymentExceedsBalanceException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPaymentAjax(int invoiceId, decimal amount, string paymentMethod)
    {
        try
        {
            await _invoiceService.ProcessPaymentAsync(invoiceId, amount, paymentMethod);
            return Json(new { success = true, message = "Payment of ₹" + amount.ToString("N2") + " processed successfully." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}