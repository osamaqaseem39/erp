using ERP.Domain;

namespace ERP.Domain.HRM;

public class LeaveType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public decimal MaxAllowedPerYear { get; set; }
    public bool RequiresApproval { get; set; }
    
    // Navigation properties
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
}
