using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class PerformanceReviewDto : BaseDto
{
    public Guid? EmployeeId { get; set; }
    public Guid? ReviewerId { get; set; }
    public DateTime? ReviewPeriodStart { get; set; }
    public DateTime? ReviewPeriodEnd { get; set; }
    public DateTime? ReviewDate { get; set; }
    public decimal? OverallRating { get; set; }
    public string? GoalsAchieved { get; set; }
    public string? GoalsNotAchieved { get; set; }
    public string? Strengths { get; set; }
    public string? AreasForImprovement { get; set; }
    public string? Comments { get; set; }
    public string? Status { get; set; } = "Draft";
    public new bool? IsActive { get; set; } = true;
    public string? EmployeeName { get; set; }
    public string? ReviewerName { get; set; }
    public Guid? CompanyId { get; set; }
}

public class CreatePerformanceReviewDto : CreateDto
{
    public Guid? EmployeeId { get; set; }
    public Guid? ReviewerId { get; set; }
    public DateTime? ReviewPeriodStart { get; set; }
    public DateTime? ReviewPeriodEnd { get; set; }
    public DateTime? ReviewDate { get; set; }
    public decimal? OverallRating { get; set; }
    public string? GoalsAchieved { get; set; }
    public string? GoalsNotAchieved { get; set; }
    public string? Strengths { get; set; }
    public string? AreasForImprovement { get; set; }
    public string? Comments { get; set; }
    public string? Status { get; set; } = "Draft";
    public Guid? CompanyId { get; set; }
}

public class UpdatePerformanceReviewDto : UpdateDto
{
    public Guid? EmployeeId { get; set; }
    public Guid? ReviewerId { get; set; }
    public DateTime? ReviewPeriodStart { get; set; }
    public DateTime? ReviewPeriodEnd { get; set; }
    public DateTime? ReviewDate { get; set; }
    public decimal? OverallRating { get; set; }
    public string? GoalsAchieved { get; set; }
    public string? GoalsNotAchieved { get; set; }
    public string? Strengths { get; set; }
    public string? AreasForImprovement { get; set; }
    public string? Comments { get; set; }
    public string? Status { get; set; } = "Draft";
}