using ERP.Domain;

namespace ERP.Domain.HRM;

public class Employee : BaseEntity
{
    public Guid? CompanyId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? Salutation { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PreferredName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? NationalId { get; set; } // Encrypted
    public string? Nationality { get; set; }
    public string? MaritalStatus { get; set; }
    public string? JobTitle { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public string? EmploymentType { get; set; }
    public string? WorkEmail { get; set; }
    public string? PersonalEmail { get; set; } // Encrypted
    public string? WorkPhone { get; set; }
    public string? MobilePhone { get; set; }
    public Guid? WorkLocationId { get; set; }
    public DateTime? DateHired { get; set; }
    public DateTime? DateTerminated { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsRemote { get; set; }
    public Guid? EmploymentContractId { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public Employee? Manager { get; set; }
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public EmploymentContract? EmploymentContract { get; set; }
    public ICollection<EmployeeAddress> Addresses { get; set; } = new List<EmployeeAddress>();
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();
    public ICollection<PayrollLine> PayrollLines { get; set; } = new List<PayrollLine>();
}
