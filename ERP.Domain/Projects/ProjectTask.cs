using ERP.Domain;
using ERP.Domain.HRM;

namespace ERP.Domain.Projects;

public class ProjectTask : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed, OnHold, Cancelled
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal Progress { get; set; } // Percentage
    public string? Notes { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public Employee? AssignedTo { get; set; }
    public ProjectTask? ParentTask { get; set; }
    public ICollection<ProjectTask> Subtasks { get; set; } = new List<ProjectTask>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}