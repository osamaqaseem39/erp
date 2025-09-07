using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class JobOpeningDto : BaseDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = "FullTime";
    public string ExperienceLevel { get; set; } = string.Empty;
    public string SkillsRequired { get; set; } = string.Empty;
    public string SalaryRange { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime PostedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public Guid PostedBy { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Benefits { get; set; }
    public Guid? HiringManagerId { get; set; }
}

public class CreateJobOpeningDto : CreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = "FullTime";
    public string ExperienceLevel { get; set; } = string.Empty;
    public string SkillsRequired { get; set; } = string.Empty;
    public string SalaryRange { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime PostedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Benefits { get; set; }
    public Guid? HiringManagerId { get; set; }
}

public class UpdateJobOpeningDto : UpdateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Department { get; set; }
    public string? Location { get; set; }
    public string? EmploymentType { get; set; }
    public string? ExperienceLevel { get; set; }
    public string? SkillsRequired { get; set; }
    public string? SalaryRange { get; set; }
    public string? Status { get; set; }
    public DateTime? PostedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Benefits { get; set; }
    public Guid? HiringManagerId { get; set; }
}