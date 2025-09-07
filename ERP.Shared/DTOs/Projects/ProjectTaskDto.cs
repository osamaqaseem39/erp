using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Projects;

public class ProjectTaskDto : BaseDto
{
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = "NotStarted";
    public string Priority { get; set; } = "Medium";
    public decimal Progress { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public string? Notes { get; set; }
}

public class CreateProjectTaskDto : CreateDto
{
    public Guid ProjectId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Status { get; set; } = "NotStarted";
    public string? Priority { get; set; } = "Medium";
    public decimal? EstimatedHours { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProjectTaskDto : UpdateDto
{
    public string? TaskName { get; set; }
    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public decimal? Progress { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public string? Notes { get; set; }
}