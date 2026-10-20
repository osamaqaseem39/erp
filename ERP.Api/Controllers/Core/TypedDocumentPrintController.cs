using ERP.Api.OpenApi;
using ERP.Api.Security;
using ERP.Api.Security.Permissions;
using ERP.Application.Core;
using ERP.Application.Interfaces.CRM;
using ERP.Application.Interfaces.Finance;
using ERP.Application.Interfaces.Procurement;
using ERP.Application.Interfaces.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Core;

/// <summary>Server-side typed prints — loads the document and builds PDF lines.</summary>
[ApiController]
[Route("api/core/prints")]
[Authorize]
[RequirePermission("reports.print.view")]
[ProducesUnauthorized]
[ProducesForbidden]
[ProducesNotFound]
public class TypedDocumentPrintController : ERP.Api.Abstractions.ControllerBase
{
    private readonly IPdfDocumentService _pdf;
    private readonly ISalesOrderService _salesOrders;
    private readonly ISalesInvoiceService _salesInvoices;
    private readonly ISalesCreditNoteService _creditNotes;
    private readonly IPurchaseOrderService _purchaseOrders;
    private readonly IDispatchAdviceService _dispatch;
    private readonly IAccountingService _accounting;

    public TypedDocumentPrintController(
        IPdfDocumentService pdf,
        ISalesOrderService salesOrders,
        ISalesInvoiceService salesInvoices,
        ISalesCreditNoteService creditNotes,
        IPurchaseOrderService purchaseOrders,
        IDispatchAdviceService dispatch,
        IAccountingService accounting,
        ILogger<TypedDocumentPrintController> logger) : base(logger)
    {
        _pdf = pdf;
        _salesOrders = salesOrders;
        _salesInvoices = salesInvoices;
        _creditNotes = creditNotes;
        _purchaseOrders = purchaseOrders;
        _dispatch = dispatch;
        _accounting = accounting;
    }

    [HttpGet("sales-orders/{id:guid}")]
    [ProducesResponseType(typeof(FileContentResult), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PrintSalesOrder(Guid id)
    {
        var tenantId = HttpContext.RequireTenantId();
        var doc = await _salesOrders.GetByIdAsync(id, tenantId);
        if (doc == null) return NotFound();
        var number = string.IsNullOrWhiteSpace(doc.OrderNumber) ? doc.SalesOrderNumber : doc.OrderNumber;
        var lines = new List<string>
        {
            $"Order: {number}",
            $"Customer: {doc.CustomerName ?? doc.CustomerId.ToString()}",
            $"Date: {doc.OrderDate:yyyy-MM-dd}  Status: {doc.Status}",
            $"Total: {doc.TotalAmount:N2}",
            "— Lines —"
        };
        foreach (var l in doc.SalesOrderLines ?? [])
            lines.Add($"{l.Description ?? l.ItemId.ToString()}  Qty {l.Quantity} × {l.UnitPrice:N2}");
        return Pdf($"SO-{number}", "Sales Order", lines);
    }

    [HttpGet("sales-invoices/{id:guid}")]
    [ProducesResponseType(typeof(FileContentResult), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PrintSalesInvoice(Guid id)
    {
        var tenantId = HttpContext.RequireTenantId();
        var doc = await _salesInvoices.GetByIdAsync(id, tenantId);
        if (doc == null) return NotFound();
        var lines = new List<string>
        {
            $"Invoice: {doc.InvoiceNumber}",
            $"Customer: {doc.CustomerName ?? doc.CustomerId.ToString()}",
            $"Date: {doc.InvoiceDate:yyyy-MM-dd}  Due: {doc.DueDate:yyyy-MM-dd}  Status: {doc.Status}",
            $"SubTotal: {doc.SubTotal:N2}  Tax: {doc.TaxAmount:N2}  Total: {doc.TotalAmount:N2}",
            $"Paid: {doc.PaidAmount:N2}  Balance: {doc.BalanceAmount:N2}",
            "— Lines —"
        };
        foreach (var l in doc.Lines ?? [])
            lines.Add($"{l.Description ?? l.ItemId.ToString()}  Qty {l.Quantity} × {l.UnitPrice:N2}");
        return Pdf($"SI-{doc.InvoiceNumber}", "Sales Invoice", lines);
    }

    [HttpGet("sales-credit-notes/{id:guid}")]
    [ProducesResponseType(typeof(FileContentResult), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PrintCreditNote(Guid id)
    {
        var tenantId = HttpContext.RequireTenantId();
        var doc = await _creditNotes.GetByIdAsync(id, tenantId);
        if (doc == null) return NotFound();
        var lines = new List<string>
        {
            $"Credit Note: {doc.CreditNoteNumber}",
            $"Customer: {doc.CustomerId}",
            $"Date: {doc.CreditNoteDate:yyyy-MM-dd}  Status: {doc.Status}",
            $"Total: {doc.TotalAmount:N2}",
            "— Lines —"
        };
        foreach (var l in doc.Lines ?? [])
            lines.Add($"{l.Description ?? l.ItemId.ToString()}  Qty {l.Quantity} × {l.UnitPrice:N2}");
        return Pdf($"CN-{doc.CreditNoteNumber}", "Sales Credit Note", lines);
    }

    [HttpGet("purchase-orders/{id:guid}")]
    [ProducesResponseType(typeof(FileContentResult), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PrintPurchaseOrder(Guid id)
    {
        var tenantId = HttpContext.RequireTenantId();
        var doc = await _purchaseOrders.GetByIdAsync(id, tenantId);
        if (doc == null) return NotFound();
        var details = doc.Details ?? doc.PurchaseOrderDetails ?? [];
        var lines = new List<string>
        {
            $"PO: {doc.PurchaseOrderNumber}",
            $"Vendor: {doc.VendorName ?? doc.VendorId.ToString()}",
            $"Date: {doc.OrderDate:yyyy-MM-dd}  Status: {doc.Status}",
            $"Total: {doc.TotalAmount:N2}",
            "— Lines —"
        };
        foreach (var l in details)
            lines.Add($"{l.ItemName ?? l.ItemId.ToString()}  Qty {l.Quantity} × {l.UnitPrice:N2}");
        return Pdf($"PO-{doc.PurchaseOrderNumber}", "Purchase Order", lines);
    }

    [HttpGet("dispatch-advices/{id:guid}")]
    [ProducesResponseType(typeof(FileContentResult), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PrintDispatchAdvice(Guid id)
    {
        var tenantId = HttpContext.RequireTenantId();
        var doc = await _dispatch.GetByIdAsync(id, tenantId);
        if (doc == null) return NotFound();
        var lines = new List<string>
        {
            $"Dispatch: {doc.AdviceNumber}",
            $"Customer: {doc.CustomerId}  SO: {doc.SalesOrderId}",
            $"Date: {doc.AdviceDate:yyyy-MM-dd}  Status: {doc.Status}",
            "— Lines —"
        };
        foreach (var l in doc.Lines ?? [])
            lines.Add($"{l.ItemId}  Qty {l.Quantity}");
        return Pdf($"DA-{doc.AdviceNumber}", "Dispatch Advice", lines);
    }

    [HttpGet("journals/{id:guid}")]
    [ProducesResponseType(typeof(FileContentResult), OpenApiResponseTypes.Ok)]
    public async Task<IActionResult> PrintJournal(Guid id)
    {
        var tenantId = HttpContext.RequireTenantId();
        var doc = await _accounting.GetJournalByIdAsync(id, tenantId);
        if (doc == null) return NotFound();
        var journalKey = doc.JournalId is Guid jid && jid != Guid.Empty ? jid : id;
        var entries = (await _accounting.GetJournalEntriesByJournalIdAsync(journalKey, tenantId)).ToList();
        var lines = new List<string>
        {
            $"Journal: {doc.JournalNumber ?? doc.Id.ToString()}",
            $"Type: {doc.JournalType}  Date: {doc.JournalDate:yyyy-MM-dd}  Status: {doc.Status}",
            $"Description: {doc.Description}",
            "— Entries —"
        };
        foreach (var e in entries)
            lines.Add($"{e.AccountCode} {e.AccountName}  Dr {e.Debit:N2}  Cr {e.Credit:N2}  {e.Narration}");
        return Pdf($"JV-{doc.JournalNumber}", "Journal Voucher", lines);
    }

    private IActionResult Pdf(string fileStem, string title, IReadOnlyList<string> lines)
    {
        var bytes = _pdf.BuildSimpleDocument(title, lines);
        return File(bytes, "application/pdf", $"{Sanitize(fileStem)}.pdf");
    }

    private static string Sanitize(string title) =>
        string.Join("_", (title ?? "document").Split(System.IO.Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
}
