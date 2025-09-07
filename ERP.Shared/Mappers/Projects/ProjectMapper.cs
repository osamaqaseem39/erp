using ERP.Domain.Projects;
using ERP.Shared.DTOs.Projects;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Projects;

public static class ProjectMapper
{
    public static ProjectDto ToDto(Project entity)
    {
        return new ProjectDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            ProjectCode = entity.ProjectCode,
            ProjectName = entity.ProjectName,
            Description = entity.Description,
            ClientId = entity.ClientId,
            ProjectManagerId = entity.ProjectManagerId,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            ActualStartDate = entity.ActualStartDate,
            ActualEndDate = entity.ActualEndDate,
            Status = entity.Status,
            Priority = entity.Priority,
            Budget = entity.Budget,
            ActualCost = entity.ActualCost,
            Progress = entity.Progress,
            Notes = entity.Notes,
            ClientName = entity.Client?.CompanyName,
            ProjectManagerName = $"{entity.ProjectManager?.FirstName} {entity.ProjectManager?.LastName}".Trim(),
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static Project ToEntity(CreateProjectDto dto, Guid tenantId, string createdBy)
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            ProjectCode = dto.ProjectCode,
            ProjectName = dto.ProjectName,
            Description = dto.Description,
            ClientId = dto.ClientId,
            ProjectManagerId = dto.ProjectManagerId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status ?? "Planning",
            Priority = dto.Priority ?? "Medium",
            Budget = dto.Budget ?? 0m,
            ActualCost = 0,
            Progress = 0,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateProjectDto dto, Project entity, string updatedBy)
    {
        if (!string.IsNullOrEmpty(dto.ProjectCode))
            entity.ProjectCode = dto.ProjectCode;
        
        if (!string.IsNullOrEmpty(dto.ProjectName))
            entity.ProjectName = dto.ProjectName;
        
        entity.Description = dto.Description;
        
        if (dto.ClientId.HasValue)
            entity.ClientId = dto.ClientId;
        
        if (dto.ProjectManagerId.HasValue)
            entity.ProjectManagerId = dto.ProjectManagerId;
        
        if (dto.StartDate.HasValue)
            entity.StartDate = dto.StartDate;
        
        if (dto.EndDate.HasValue)
            entity.EndDate = dto.EndDate;
        
        if (!string.IsNullOrEmpty(dto.Status))
            entity.Status = dto.Status;
        
        if (!string.IsNullOrEmpty(dto.Priority))
            entity.Priority = dto.Priority;
        
        if (dto.Budget.HasValue)
            entity.Budget = dto.Budget.Value;
        
        entity.Notes = dto.Notes;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}