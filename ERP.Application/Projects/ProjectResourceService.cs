using ERP.Application.Interfaces.Projects;
using ERP.Domain.Projects;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Projects;

public class ProjectResourceService : IProjectResourceService
{
    private readonly IProjectResourceRepository _repository;
    private readonly IProjectRepository _projects;

    public ProjectResourceService(IProjectResourceRepository repository, IProjectRepository projects)
    {
        _repository = repository;
        _projects = projects;
    }

    public async Task<ProjectResourceDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repository.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<ProjectResourceDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectResourceDto>();
        return (await _repository.GetAllAsync(tenantId)).Select(Map);
    }

    public async Task<IEnumerable<ProjectResourceDto>> GetByProjectAsync(Guid projectId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectResourceDto>();
        return (await _repository.GetByProjectAsync(projectId, tenantId)).Select(Map);
    }

    public async Task<IEnumerable<ProjectResourceDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectResourceDto>();
        return (await _repository.GetByEmployeeAsync(employeeId, tenantId)).Select(Map);
    }

    public async Task<Guid> CreateAsync(CreateProjectResourceDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || dto.ProjectId == Guid.Empty) return Guid.Empty;
        var project = await _projects.GetByIdAsync(dto.ProjectId, tenantId);
        if (project == null) return Guid.Empty;

        var name = !string.IsNullOrWhiteSpace(dto.Role) ? dto.Role.Trim()
            : dto.ResourceId != Guid.Empty ? dto.ResourceId.ToString("N")[..8]
            : "Resource";
        var entity = new ProjectResource
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = dto.ProjectId,
            EmployeeId = dto.ResourceType?.Equals("Employee", StringComparison.OrdinalIgnoreCase) == true
                ? dto.ResourceId
                : null,
            ResourceType = string.IsNullOrWhiteSpace(dto.ResourceType) ? "Employee" : dto.ResourceType,
            ResourceName = name,
            AllocatedHours = dto.AllocatedHours ?? 0,
            HourlyRate = dto.HourlyRate ?? 0,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status!,
            Notes = dto.Notes,
            CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateProjectResourceDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        if (dto.ResourceType != null) e.ResourceType = dto.ResourceType;
        if (dto.ResourceId.HasValue && dto.ResourceId != Guid.Empty
            && e.ResourceType.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            e.EmployeeId = dto.ResourceId;
        if (dto.Role != null) e.ResourceName = dto.Role;
        if (dto.AllocatedHours.HasValue) e.AllocatedHours = dto.AllocatedHours.Value;
        if (dto.ActualHours.HasValue) e.ActualHours = dto.ActualHours.Value;
        if (dto.HourlyRate.HasValue) e.HourlyRate = dto.HourlyRate.Value;
        // Status not free-set on Update
        if (dto.Notes != null) e.Notes = dto.Notes;
        e.UpdatedBy = userId != Guid.Empty ? userId.ToString() : dto.UpdatedBy;
        e.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    private static ProjectResourceDto Map(ProjectResource e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ProjectId = e.ProjectId,
        ProjectName = e.Project?.ProjectName,
        ResourceId = e.EmployeeId ?? e.Id,
        ResourceName = e.ResourceName,
        ResourceType = e.ResourceType,
        Role = e.ResourceName,
        AllocatedHours = e.AllocatedHours,
        ActualHours = e.ActualHours,
        HourlyRate = e.HourlyRate,
        Status = e.Status,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
