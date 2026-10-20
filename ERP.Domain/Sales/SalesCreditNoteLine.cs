using ERP.Domain;

namespace ERP.Domain.Sales;

public class SalesCreditNoteLine : BaseEntity
{
    public Guid CreditNoteId { get; set; }
    public int LineNumber { get; set; }
    public Guid? OriginalSalesInvoiceLineId { get; set; }
    public Guid? ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
}
