using ERP.Application.Common;
using ERP.Application.Interfaces.Projects;
using ERP.Domain.Projects;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Projects;

public class ProjectMilestoneService : IProjectMilestoneService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Pending"] = new[] { "Completed", "Cancelled" },
            ["Completed"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IProjectMilestoneRepository _repository;
    private readonly IProjectRepository _projects;

    public ProjectMilestoneService(IProjectMilestoneRepository repository, IProjectRepository projects)
    {
        _repository = repository;
        _projects = projects;
    }

    public async Task<ProjectMilestoneDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repository.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<ProjectMilestoneDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectMilestoneDto>();
        return (await _repository.GetAllAsync(tenantId)).Select(Map);
    }

    public async Task<IEnumerable<ProjectMilestoneDto>> GetByProjectAsync(Guid projectId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectMilestoneDto>();
        return (await _repository.GetByProjectAsync(projectId, tenantId)).Select(Map);
    }

    public async Task<IEnumerable<ProjectMilestoneDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectMilestoneDto>();
        return (await _repository.GetByStatusAsync(status, tenantId)).Select(Map)
            .Where(m => string.Equals(m.Status, status, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Guid> CreateAsync(CreateProjectMilestoneDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || dto.ProjectId == Guid.Empty || string.IsNullOrWhiteSpace(dto.MilestoneName))
            return Guid.Empty;
        var project = await _projects.GetByIdAsync(dto.ProjectId, tenantId);
        if (project == null) return Guid.Empty;

        var entity = new ProjectMilestone
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = dto.ProjectId,
            MilestoneName = dto.MilestoneName.Trim(),
            Description = dto.Description,
            TargetDate = dto.TargetDate,
            // ERP-FIX-5: always start Pending; status only via CompleteAsync
            Status = "Pending",
            Notes = dto.Notes,
            CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateProjectMilestoneDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        if (string.Equals(e.Status, "Completed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(e.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            return false;
        if (dto.MilestoneName != null) e.MilestoneName = dto.MilestoneName;
        if (dto.Description != null) e.Description = dto.Description;
        if (dto.TargetDate.HasValue) e.TargetDate = dto.TargetDate;
        if (dto.Notes != null) e.Notes = dto.Notes;
        // ERP-FIX-5: status only via CompleteAsync
        e.UpdatedBy = userId != Guid.Empty ? userId.ToString() : dto.UpdatedBy;
        e.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(id, tenantId);
        if (e == null) return false;
        if (string.Equals(e.Status, "Completed", StringComparison.OrdinalIgnoreCase)) return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(id, tenantId);
        if (e == null) return false;
        if (!DocumentWorkflow.CanTransition(e.Status, "Completed", Transitions))
            return false;
        e.Status = "Completed";
        e.ActualDate = DateTime.UtcNow;
        e.UpdatedBy = userId != Guid.Empty ? userId.ToString() : e.UpdatedBy;
        e.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(e);
    }

    private static ProjectMilestoneDto Map(ProjectMilestone e)
    {
        var status = e.Status;
        if (!string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            && e.TargetDate.HasValue
            && e.TargetDate.Value.Date < DateTime.UtcNow.Date)
            status = "Overdue";

        return new ProjectMilestoneDto
        {
            Id = e.Id,
            TenantId = e.TenantId,
            ProjectId = e.ProjectId,
            ProjectName = e.Project?.ProjectName,
            MilestoneName = e.MilestoneName,
            Description = e.Description,
            TargetDate = e.TargetDate,
            CompletedDate = e.ActualDate,
            Status = status,
            Notes = e.Notes,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}
