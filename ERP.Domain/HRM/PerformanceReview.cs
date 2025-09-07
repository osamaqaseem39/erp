using ERP.Domain;

namespace ERP.Domain.HRM;

public class PerformanceReview : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public DateTime ReviewPeriodStart { get; set; }
    public DateTime ReviewPeriodEnd { get; set; }
    public DateTime ReviewDate { get; set; }
    public decimal OverallRating { get; set; }
    public string? GoalsAchieved { get; set; }
    public string? GoalsNotAchieved { get; set; }
    public string? Strengths { get; set; }
    public string? AreasForImprovement { get; set; }
    public string? Comments { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Completed
    public string? GoalsJson { get; set; }
    public Guid? CompanyId { get; set; }
    
    // Navigation properties
    public Employee Employee { get; set; } = null!;
    public Employee Reviewer { get; set; } = null!;
}
