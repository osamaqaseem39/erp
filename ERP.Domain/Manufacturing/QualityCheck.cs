using ERP.Domain;

namespace ERP.Domain.Manufacturing;

public class QualityCheck : BaseEntity
{
    public Guid ProductionOrderId { get; set; }
    public string CheckNumber { get; set; } = string.Empty;
    /// <summary>Pending | Passed | Failed</summary>
    public string Status { get; set; } = "Pending";
    public DateTime? CheckedAt { get; set; }
    public string? Notes { get; set; }

    public ProductionOrder ProductionOrder { get; set; } = null!;
    public ICollection<QualityCheckLine> Lines { get; set; } = new List<QualityCheckLine>();
}

public class QualityCheckLine : BaseEntity
{
    public Guid QualityCheckId { get; set; }
    public Guid? ItemId { get; set; }
    public decimal QuantityInspected { get; set; }
    public decimal QuantityPassed { get; set; }
    public decimal QuantityFailed { get; set; }
    public string? Notes { get; set; }

    public QualityCheck QualityCheck { get; set; } = null!;
}
