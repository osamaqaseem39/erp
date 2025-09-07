using ERP.Domain;

namespace ERP.Domain.Procurement;

public class PurchaseInvoiceDetail : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; } = 0;
    public decimal NetAmount { get; set; }
    public new string CreatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual PurchaseInvoice PurchaseInvoice { get; set; } = null!;
    public virtual SCM.Item Item { get; set; } = null!;
}
