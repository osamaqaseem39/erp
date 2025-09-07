using ERP.Domain;

namespace ERP.Domain.SCM;

public class ReorderRule : BaseEntity
{
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal MinStockLevel { get; set; }
    public decimal MaxStockLevel { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal ReorderQuantity { get; set; }
    public int LeadTimeDays { get; set; } = 0;
    public new bool IsActive { get; set; } = true;
    public new Guid TenantId { get; set; }
    
    // Navigation properties
    public Item Item { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}
