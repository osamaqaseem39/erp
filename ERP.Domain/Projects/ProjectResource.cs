using ERP.Domain;
using ERP.Domain.HRM;

namespace ERP.Domain.Projects;

public class ProjectResource : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string ResourceType { get; set; } = string.Empty; // Employee, Equipment, Material
    public string ResourceName { get; set; } = string.Empty;
    public decimal AllocatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal HourlyRate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Completed
    public string? Notes { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public Employee? Employee { get; set; }
}