using ERP.Domain;

namespace ERP.Domain.Procurement;

public class PurchaseOrderDetail : BaseEntity
{
    public Guid POId { get; set; }
    public Guid ItemId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; } = 0; // percentage
    public decimal TaxAmount { get; set; } = 0;
    public decimal NetAmount { get; set; }
    public decimal ReceivedQuantity { get; set; } = 0; // updated on GRN
    public new string CreatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    public virtual SCM.Item Item { get; set; } = null!;
}
