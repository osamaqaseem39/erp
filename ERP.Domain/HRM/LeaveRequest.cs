using ERP.Domain;

namespace ERP.Domain.HRM;

public class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public decimal DaysRequested { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled
    public DateTime RequestedAt { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? Notes { get; set; }
    public string? Reason { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? CompanyId { get; set; }
    
    // Navigation properties
    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}
