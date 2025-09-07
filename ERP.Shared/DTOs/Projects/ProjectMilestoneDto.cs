using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Projects;

public class ProjectMilestoneDto : BaseDto
{
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = "NotStarted";
    public string Priority { get; set; } = "Medium";
    public decimal Progress { get; set; }
    public string? Notes { get; set; }
}

public class CreateProjectMilestoneDto : CreateDto
{
    public Guid ProjectId { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? TargetDate { get; set; }
    public string? Status { get; set; } = "NotStarted";
    public string? Priority { get; set; } = "Medium";
    public string? Notes { get; set; }
}

public class UpdateProjectMilestoneDto : UpdateDto
{
    public string? MilestoneName { get; set; }
    public string? Description { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public decimal? Progress { get; set; }
    public string? Notes { get; set; }
}