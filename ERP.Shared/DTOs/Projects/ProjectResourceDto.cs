using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Projects;

public class ProjectResourceDto : BaseDto
{
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid ResourceId { get; set; }
    public string? ResourceName { get; set; }
    public string ResourceType { get; set; } = string.Empty; // Employee, Equipment, Material
    public string Role { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal AllocatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal HourlyRate { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class CreateProjectResourceDto : CreateDto
{
    public Guid ProjectId { get; set; }
    public Guid ResourceId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? AllocatedHours { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class UpdateProjectResourceDto : UpdateDto
{
    public Guid? ResourceId { get; set; }
    public string? ResourceType { get; set; }
    public string? Role { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? AllocatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}