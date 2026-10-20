using ERP.Domain;

namespace ERP.Domain.Projects;

public class ProjectTimesheet : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal TotalHours { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? Notes { get; set; }

    public Project? Project { get; set; }
    public ICollection<ProjectTimesheetLine> Lines { get; set; } = new List<ProjectTimesheetLine>();
}

public class ProjectTimesheetLine : BaseEntity
{
    public Guid ProjectTimesheetId { get; set; }
    public int LineNumber { get; set; }
    public DateTime WorkDate { get; set; }
    public Guid? ProjectTaskId { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public bool IsBillable { get; set; } = true;
    public decimal HourlyRate { get; set; }

    public ProjectTimesheet ProjectTimesheet { get; set; } = null!;
}
