using ERP.Domain;

namespace ERP.Domain.HRM;

public class EmploymentContract : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string ContractType { get; set; } = string.Empty; // Permanent, FixedTerm, Probation
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal WorkingHoursPerWeek { get; set; }
    public decimal BaseSalary { get; set; }
    public string Currency { get; set; } = string.Empty; // ISO 4217
    public string PayFrequency { get; set; } = string.Empty; // Monthly/Weekly/Biweekly
    public int NoticePeriodDays { get; set; }
    public int ProbationPeriodDays { get; set; }
    public string? ContractDocumentUrl { get; set; }
    public string Status { get; set; } = "Active"; // Active, Expired, Terminated
    
    // Navigation property
    public Employee Employee { get; set; } = null!;
}
