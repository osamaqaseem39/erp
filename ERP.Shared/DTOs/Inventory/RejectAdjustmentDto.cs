using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class RejectAdjustmentDto
{
    public Guid AdjustmentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}