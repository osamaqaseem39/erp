using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class ApproveLeaveRequestDto
{
    public Guid LeaveRequestId { get; set; }
    public string? Comments { get; set; }
}