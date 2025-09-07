using ERP.Domain;

namespace ERP.Domain.HRM;

public class JobOpening : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public string SkillsRequired { get; set; } = string.Empty;
    public string SalaryRange { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public DateTime PostedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public string Status { get; set; } = "Open"; // Open, Closed, Filled
    public Guid PostedBy { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Benefits { get; set; }
    public Guid? HiringManagerId { get; set; }
    
    // Navigation properties
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}
