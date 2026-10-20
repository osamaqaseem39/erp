using ERP.Domain;

namespace ERP.Domain.CRM;

public class SalesOrderLine : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal Tax1Rate { get; set; }
    public decimal Tax1Amount { get; set; }
    public decimal Tax2Rate { get; set; }
    public decimal Tax2Amount { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public SalesOrder SalesOrder { get; set; } = null!;
}