using ERP.Application.Common;
using ERP.Application.Interfaces.Projects;
using ERP.Domain.Projects;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Projects;

public class ProjectTaskService : IProjectTaskService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["NotStarted"] = new[] { "InProgress", "Completed", "Cancelled" },
            ["InProgress"] = new[] { "Completed", "Cancelled" },
            ["Completed"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IProjectTaskRepository _repository;
    private readonly IProjectRepository _projects;

    public ProjectTaskService(IProjectTaskRepository repository, IProjectRepository projects)
    {
        _repository = repository;
        _projects = projects;
    }

    public async Task<ProjectTaskDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repository.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<ProjectTaskDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetAllAsync(tenantId)).Select(Map);
    }

    public async Task<IEnumerable<ProjectTaskDto>> GetByProjectAsync(Guid projectId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetByProjectAsync(projectId, tenantId)).Select(Map);
    }

    public async Task<IEnumerable<ProjectTaskDto>> GetByAssignedToAsync(Guid assignedToId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetByAssignedToAsync(assignedToId, tenantId)).Select(Map);
    }

    public async Task<IEnumerable<ProjectTaskDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetByStatusAsync(status, tenantId)).Select(Map);
    }

    public async Task<Guid> CreateAsync(CreateProjectTaskDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || dto.ProjectId == Guid.Empty || string.IsNullOrWhiteSpace(dto.TaskName))
            return Guid.Empty;
        var project = await _projects.GetByIdAsync(dto.ProjectId, tenantId);
        if (project == null) return Guid.Empty;

        var entity = new ProjectTask
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = dto.ProjectId,
            TaskName = dto.TaskName.Trim(),
            Description = dto.Description,
            AssignedToId = dto.AssignedToId,
            StartDate = dto.StartDate,
            DueDate = dto.DueDate,
            // ERP-FIX-5: always start NotStarted; status only via CompleteAsync
            Status = "NotStarted",
            Priority = string.IsNullOrWhiteSpace(dto.Priority) ? "Medium" : dto.Priority!,
            EstimatedHours = dto.EstimatedHours ?? 0,
            Notes = dto.Notes,
            CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateProjectTaskDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        if (IsTerminal(e.Status)) return false;

        if (dto.TaskName != null) e.TaskName = dto.TaskName;
        if (dto.Description != null) e.Description = dto.Description;
        if (dto.AssignedToId.HasValue) e.AssignedToId = dto.AssignedToId;
        if (dto.StartDate.HasValue) e.StartDate = dto.StartDate;
        if (dto.DueDate.HasValue) e.DueDate = dto.DueDate;
        // ERP-FIX-5: status only via CompleteAsync
        if (dto.Priority != null) e.Priority = dto.Priority;
        if (dto.Progress.HasValue) e.Progress = dto.Progress.Value;
        if (dto.EstimatedHours.HasValue) e.EstimatedHours = dto.EstimatedHours.Value;
        if (dto.ActualHours.HasValue) e.ActualHours = dto.ActualHours.Value;
        if (dto.Notes != null) e.Notes = dto.Notes;
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
        e.Progress = 100;
        e.CompletedDate = DateTime.UtcNow;
        e.UpdatedBy = userId == Guid.Empty ? e.UpdatedBy : userId.ToString();
        e.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(e);
    }

    private static bool IsTerminal(string? status) =>
        string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);

    private static ProjectTaskDto Map(ProjectTask e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ProjectId = e.ProjectId,
        ProjectName = e.Project?.ProjectName,
        TaskName = e.TaskName,
        Description = e.Description,
        AssignedToId = e.AssignedToId,
        AssignedToName = e.AssignedTo == null ? null : $"{e.AssignedTo.FirstName} {e.AssignedTo.LastName}".Trim(),
        StartDate = e.StartDate,
        DueDate = e.DueDate,
        CompletedDate = e.CompletedDate,
        Status = e.Status,
        Priority = e.Priority,
        Progress = e.Progress,
        EstimatedHours = e.EstimatedHours,
        ActualHours = e.ActualHours,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
