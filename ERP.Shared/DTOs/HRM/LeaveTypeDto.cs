using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class LeaveTypeDto : BaseDto
{
    public new Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public decimal MaxAllowedPerYear { get; set; }
    public bool RequiresApproval { get; set; }
    public new bool IsActive { get; set; } = true;
}

public class CreateLeaveTypeDto : CreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public decimal MaxAllowedPerYear { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public new bool IsActive { get; set; } = true;
}

public class UpdateLeaveTypeDto : UpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public decimal MaxAllowedPerYear { get; set; }
    public bool RequiresApproval { get; set; }
    public new bool IsActive { get; set; } = true;
}
