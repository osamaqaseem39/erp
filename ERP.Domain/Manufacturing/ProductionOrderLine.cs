using ERP.Domain;
using ERP.Domain.SCM;

namespace ERP.Domain.Manufacturing;

public class ProductionOrderLine : BaseEntity
{
    public Guid ProductionOrderId { get; set; }
    public Guid? ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public string LineType { get; set; } = string.Empty; // Material, Labor, Overhead
    public string? Notes { get; set; }
    
    // Navigation properties
    public ProductionOrder ProductionOrder { get; set; } = null!;
    public Item? Item { get; set; }
}