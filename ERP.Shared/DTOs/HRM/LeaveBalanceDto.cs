using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class LeaveBalanceDto : BaseDto
{
    public new Guid? Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }
    public decimal Accrued { get; set; }
    public decimal Taken { get; set; }
    public decimal Available { get; set; }
}
