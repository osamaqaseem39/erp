using ERP.Domain;
using ERP.Domain.SCM;

namespace ERP.Domain.Manufacturing;

public class ProductionOrder : BaseEntity
{
    public Guid? CompanyId { get; set; }
    public string ProductionOrderNumber { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? ItemId { get; set; }
    public Guid? WorkCenterId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string Status { get; set; } = "Planned"; // Planned, InProgress, Completed, Cancelled
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public decimal CompletedQuantity { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    
    // Navigation properties
    public Item? Item { get; set; }
    public ICollection<ProductionOrderLine> ProductionOrderLines { get; set; } = new List<ProductionOrderLine>();
    public ICollection<ProductionRun> ProductionRuns { get; set; } = new List<ProductionRun>();
}