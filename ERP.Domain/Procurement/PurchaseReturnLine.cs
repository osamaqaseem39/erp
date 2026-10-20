using ERP.Domain;

namespace ERP.Domain.Procurement;

public class PurchaseReturnLine : BaseEntity
{
    public Guid PurchaseReturnId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Reason { get; set; }

    public PurchaseReturn? PurchaseReturn { get; set; }
}
