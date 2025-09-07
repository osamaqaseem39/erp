using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Manufacturing;

public class CompleteProductionOrderDto
{
    public Guid ProductionOrderId { get; set; }
    public DateTime CompletionDate { get; set; }
    public decimal ActualQuantity { get; set; }
    public string? Notes { get; set; }
}