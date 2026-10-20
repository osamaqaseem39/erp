using ERP.Domain;

namespace ERP.Domain.Projects;

public class ProjectDocument : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty; // Specification, Design, Report, Contract
    public string FilePath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long FileSize { get; set; }
    public string? Version { get; set; }
    public bool IsPublic { get; set; } = false;
    public Guid? ProjectTaskId { get; set; }
    public Guid? ProjectMilestoneId { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
}