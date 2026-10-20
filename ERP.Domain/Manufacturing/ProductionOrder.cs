using ERP.Domain;
using ERP.Domain.SCM;

namespace ERP.Domain.Manufacturing;

public class ProductionOrder : BaseEntity
{
    public Guid? CompanyId { get; set; }
    public string ProductionOrderNumber { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? ItemId { get; set; }
    public Guid? BomId { get; set; }
    public Guid? RoutingId { get; set; }
    public Guid? WorkCenterId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public Guid? SalesOrderLineId { get; set; }
    public Guid? WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    /// <summary>Draft | Planned | Released | InProgress | QualityCheck | Completed | Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public string Priority { get; set; } = "Medium";
    public decimal CompletedQuantity { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }

    public Item? Item { get; set; }
    public BillOfMaterial? BillOfMaterial { get; set; }
    public Routing? Routing { get; set; }
    public WorkCenter? WorkCenter { get; set; }
    public ICollection<ProductionOrderLine> ProductionOrderLines { get; set; } = new List<ProductionOrderLine>();
    public ICollection<ProductionRun> ProductionRuns { get; set; } = new List<ProductionRun>();
    public ICollection<ProductionScrap> ProductionScraps { get; set; } = new List<ProductionScrap>();
    public ICollection<ProductionOrderOperation> Operations { get; set; } = new List<ProductionOrderOperation>();
    public ICollection<QualityCheck> QualityChecks { get; set; } = new List<QualityCheck>();
}
