using ERP.Domain;

namespace ERP.Domain.Manufacturing;

public class ProductionOrderOperation : BaseEntity
{
    public Guid ProductionOrderId { get; set; }
    public Guid? RoutingStepId { get; set; }
    public int StepNumber { get; set; }
    public Guid? WorkCenterId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public decimal StandardTimeMinutes { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }
    public decimal? ActualTimeMinutes { get; set; }
    /// <summary>Pending | InProgress | Completed | Skipped</summary>
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }

    public ProductionOrder ProductionOrder { get; set; } = null!;
}
