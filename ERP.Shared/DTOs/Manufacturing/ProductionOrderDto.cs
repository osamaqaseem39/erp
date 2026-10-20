using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Manufacturing;

public class ProductionOrderDto : BaseDto
{
    public Guid? Id { get; set; }
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
    public string? ItemName { get; set; }
    public string? CancellationReason { get; set; }
    public List<ProductionOrderLineDto> Lines { get; set; } = [];
    public List<ProductionOrderOperationDto> Operations { get; set; } = [];
}

public class CreateProductionOrderDto : CreateDto
{
    public Guid? CompanyId { get; set; }
    public string? ProductionOrderNumber { get; set; }
    public string? OrderNumber { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? BomId { get; set; }
    public Guid? RoutingId { get; set; }
    public Guid? WorkCenterId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public Guid? SalesOrderLineId { get; set; }
    public Guid? WarehouseId { get; set; }
    public decimal? Quantity { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; } = "Medium";
    public string? Notes { get; set; }
}

public class UpdateProductionOrderDto : UpdateDto
{
    public string? ProductionOrderNumber { get; set; }
    public string? OrderNumber { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? BomId { get; set; }
    public Guid? RoutingId { get; set; }
    public Guid? WorkCenterId { get; set; }
    public Guid? WarehouseId { get; set; }
    public decimal? Quantity { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Notes { get; set; }
}

public class ProductionOrderOperationDto
{
    public Guid Id { get; set; }
    public Guid ProductionOrderId { get; set; }
    public Guid? RoutingStepId { get; set; }
    public int StepNumber { get; set; }
    public Guid? WorkCenterId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public decimal StandardTimeMinutes { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }
    public decimal? ActualTimeMinutes { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
}

public class CompleteProductionOrderOperationDto
{
    public decimal? ActualTimeMinutes { get; set; }
    public string? Notes { get; set; }
}
