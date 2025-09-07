using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Manufacturing;

public class CancelProductionOrderDto
{
    public Guid ProductionOrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}