using ERP.Domain;

namespace ERP.Domain.Sales;

public class SalesCreditNote : BaseEntity
{
    public string CreditNoteNumber { get; set; } = string.Empty;
    public DateTime CreditNoteDate { get; set; }
    public Guid CustomerId { get; set; }
    public Guid OriginalSalesInvoiceId { get; set; }
    public Guid? ReturnOrderId { get; set; }
    /// <summary>When set, post receives line qty into this warehouse; void issues it back.</summary>
    public Guid? WarehouseId { get; set; }
    public bool StockReceived { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Reason { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public Guid? AppliedFinanceInvoiceId { get; set; }
    /// <summary>GL journal posted on CN post (Dr SalesReturns / Cr AR); reversed on void.</summary>
    public Guid? PostedJournalId { get; set; }
}

