using ERP.Domain;

namespace ERP.Domain.Procurement;

public class PurchaseDebitNote : BaseEntity
{
    public string DebitNoteNumber { get; set; } = string.Empty;
    public DateTime DebitNoteDate { get; set; }
    public Guid VendorId { get; set; }
    public Guid OriginalPurchaseInvoiceId { get; set; }
    public Guid? PurchaseReturnId { get; set; }
    /// <summary>When set, post issues line qty from this warehouse; void receives it back.</summary>
    public Guid? WarehouseId { get; set; }
    public bool StockIssued { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Reason { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    /// <summary>Finance AP bill credit was applied to on post.</summary>
    public Guid? AppliedFinanceBillId { get; set; }
    /// <summary>GL journal posted on DN post (Dr AP / Cr PurchaseReturns); reversed on void.</summary>
    public Guid? PostedJournalId { get; set; }
}
