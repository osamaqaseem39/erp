using ERP.Domain;

namespace ERP.Domain.SCM;

public class CycleCount : BaseEntity
{
    public string CountNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public DateTime CountDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public Guid? PostedStockAdjustmentId { get; set; }
    public DateTime? PostedAt { get; set; }
    public string? PostedBy { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<CycleCountLine> Lines { get; set; } = new List<CycleCountLine>();
}

public class CycleCountLine : BaseEntity
{
    public Guid CycleCountId { get; set; }
    public Guid ItemId { get; set; }
    public Guid? WarehouseLocationId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityCounted { get; set; }
    public decimal QuantityVariance { get; set; }
    public string? Notes { get; set; }

    public CycleCount CycleCount { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public WarehouseLocation? WarehouseLocation { get; set; }
}
