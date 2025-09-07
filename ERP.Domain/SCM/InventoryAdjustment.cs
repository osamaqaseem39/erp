using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.SCM;

public class InventoryAdjustment : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid ItemId { get; set; }
    public string AdjustmentType { get; set; } = string.Empty; // Increase, Decrease
    public decimal Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    
    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual Item Item { get; set; } = null!;
}
