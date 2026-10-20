using ERP.Domain;

namespace ERP.Domain.SCM;

public class StockAdjustment : BaseEntity
{
    public Guid? CompanyId { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public Guid WarehouseId { get; set; }
    public string AdjustmentType { get; set; } = "Physical";
    public string Status { get; set; } = "Draft"; // Draft, Posted, Cancelled
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public string? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<StockAdjustmentLine> Lines { get; set; } = new List<StockAdjustmentLine>();
}

public class StockAdjustmentLine : BaseEntity
{
    public Guid StockAdjustmentId { get; set; }
    public Guid ItemId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityCounted { get; set; }
    public decimal QuantityDifference { get; set; }
    public string? Notes { get; set; }

    public StockAdjustment StockAdjustment { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
