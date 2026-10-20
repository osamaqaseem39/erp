using ERP.Domain;

namespace ERP.Domain.Manufacturing;

public class ProductionRun : BaseEntity
{
    public Guid ProductionOrderId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal QuantityProduced { get; set; }
    public decimal QuantityRejected { get; set; }
    public Guid? WorkCenterId { get; set; }
    public Guid? ProductionOrderOperationId { get; set; }
    /// <summary>InProgress | Completed | Cancelled</summary>
    public string Status { get; set; } = "InProgress";
    public string? Notes { get; set; }

    public ProductionOrder ProductionOrder { get; set; } = null!;
}
