using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class LeaveRequestDto : BaseDto
{
    public new Guid? Id { get; set; }
    public new Guid? TenantId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DaysRequested { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public DateTime AppliedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public new string? CreatedBy { get; set; }
    public new string? UpdatedBy { get; set; }
    public string? EmployeeName { get; set; }
    public string? LeaveTypeName { get; set; }
    public string? ApprovedByUserName { get; set; }
}

public class CreateLeaveRequestDto : CreateDto
{
    public new Guid? TenantId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DaysRequested { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public DateTime AppliedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
}

public class UpdateLeaveRequestDto : UpdateDto
{
    public new Guid? TenantId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DaysRequested { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public DateTime AppliedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
}