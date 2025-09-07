using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Manufacturing;

public class ProductionOrderLineDto
{
    public Guid Id { get; set; }
    public Guid ProductionOrderId { get; set; }
    public string? ProductionOrderNumber { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal CompletedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateProductionOrderLineDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProductionOrderLineDto
{
    public Guid? ItemId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? CompletedQuantity { get; set; }
    public string? Notes { get; set; }
}