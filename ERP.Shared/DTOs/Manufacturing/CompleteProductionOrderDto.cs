using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Manufacturing;

public class CompleteProductionOrderDto
{
    public Guid ProductionOrderId { get; set; }
    public DateTime CompletionDate { get; set; }
    public decimal ActualQuantity { get; set; }
    /// <summary>Warehouse for component issue and FG receipt. Required when order has a BOM.</summary>
    public Guid? WarehouseId { get; set; }
    public string? Notes { get; set; }
}