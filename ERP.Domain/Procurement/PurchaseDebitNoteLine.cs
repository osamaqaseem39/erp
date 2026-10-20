using ERP.Domain;

namespace ERP.Domain.Procurement;

public class PurchaseDebitNoteLine : BaseEntity
{
    public Guid DebitNoteId { get; set; }
    public int LineNumber { get; set; }
    public Guid? OriginalPurchaseInvoiceLineId { get; set; }
    public Guid? ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
}
