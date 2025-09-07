using ERP.Domain;

namespace ERP.Domain.HRM;

public class LeaveBalance : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public decimal Accrued { get; set; }
    public decimal Taken { get; set; }
    public decimal Available => Accrued - Taken;
    
    // Navigation properties
    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}
