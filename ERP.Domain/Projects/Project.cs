using ERP.Domain;
using ERP.Domain.CRM;
using ERP.Domain.HRM;

namespace ERP.Domain.Projects;

public class Project : BaseEntity
{
    public Guid? CompanyId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ProjectManagerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string Status { get; set; } = "Planning"; // Planning, InProgress, OnHold, Completed, Cancelled
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public decimal Budget { get; set; }
    public decimal ActualCost { get; set; }
    public decimal Progress { get; set; } // Percentage
    public string? Notes { get; set; }
    
    // Navigation properties
    public Customer? Client { get; set; }
    public Employee? ProjectManager { get; set; }
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public ICollection<ProjectResource> Resources { get; set; } = new List<ProjectResource>();
    public ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();
    public ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
}