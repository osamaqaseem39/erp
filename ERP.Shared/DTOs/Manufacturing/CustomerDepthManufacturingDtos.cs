namespace ERP.Shared.DTOs.Manufacturing;

public class RoutingDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string RoutingNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ItemId { get; set; }
    public string Version { get; set; } = "1";
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public List<RoutingStepDto> Steps { get; set; } = new();
}

public class RoutingStepDto
{
    public Guid Id { get; set; }
    public int StepNumber { get; set; }
    public Guid? WorkCenterId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public decimal StandardTimeMinutes { get; set; }
    public string? Notes { get; set; }
}

public class CreateRoutingDto
{
    public string RoutingNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ItemId { get; set; }
    public string? Version { get; set; }
    public string? Notes { get; set; }
    public List<CreateRoutingStepDto> Steps { get; set; } = new();
}

public class CreateRoutingStepDto
{
    public int StepNumber { get; set; }
    public Guid? WorkCenterId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public decimal StandardTimeMinutes { get; set; }
    public string? Notes { get; set; }
}

public class ProductionScrapDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProductionOrderId { get; set; }
    public Guid? ItemId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime ScrapDate { get; set; }
    public string? Reason { get; set; }
}

public class CreateProductionScrapDto
{
    public Guid ProductionOrderId { get; set; }
    public Guid? ItemId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime? ScrapDate { get; set; }
    public string? Reason { get; set; }
}

public class ReleaseSalesOrderLineDto
{
    public Guid SalesOrderLineId { get; set; }
    public Guid? BomId { get; set; }
    public Guid? RoutingId { get; set; }
    public Guid? WorkCenterId { get; set; }
    public decimal? Quantity { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
}

public class PendingSalesOrderForProductionDto
{
    public Guid SalesOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid SalesOrderLineId { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReleasedQuantity { get; set; }
    public decimal PendingQuantity { get; set; }
}
