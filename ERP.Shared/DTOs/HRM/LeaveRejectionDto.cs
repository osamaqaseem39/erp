using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class LeaveRejectionDto
{
    public Guid LeaveRequestId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Comments { get; set; }
}