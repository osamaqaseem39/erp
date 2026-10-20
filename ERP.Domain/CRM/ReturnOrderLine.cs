using ERP.Domain;

namespace ERP.Domain.CRM;

public class ReturnOrderLine : BaseEntity
{
    public Guid ReturnOrderId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Reason { get; set; }

    public ReturnOrder? ReturnOrder { get; set; }
}
