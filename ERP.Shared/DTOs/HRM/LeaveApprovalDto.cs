using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class LeaveApprovalDto
{
    public Guid LeaveRequestId { get; set; }
    public string? Notes { get; set; }
}