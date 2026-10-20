using ERP.Application.Interfaces.Projects;
using ERP.Domain.Projects;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Projects;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var project = await _projectRepository.GetByIdAsync(id, tenantId);
        return project != null ? MapToDto(project) : null;
    }

    public async Task<IEnumerable<ProjectDto>> GetAllAsync(Guid tenantId, string? status = null, Guid? managerId = null, Guid? customerId = null)
    {
        IEnumerable<Project> projects = await _projectRepository.GetAllAsync(tenantId);

        if (status != null)
            projects = projects.Where(p => p.Status == status);
        if (managerId.HasValue)
            projects = projects.Where(p => p.ProjectManagerId == managerId.Value);
        if (customerId.HasValue)
            projects = projects.Where(p => p.ClientId == customerId.Value);

        return projects.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        var projects = await _projectRepository.GetByStatusAsync(status, tenantId);
        return projects.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectDto>> GetByManagerAsync(Guid managerId, Guid tenantId)
    {
        var projects = await _projectRepository.GetByManagerAsync(managerId, tenantId);
        return projects.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectDto>> GetByClientAsync(Guid clientId, Guid tenantId)
    {
        var projects = await _projectRepository.GetByClientAsync(clientId, tenantId);
        return projects.Select(MapToDto);
    }

    public async Task<ProjectCreateResult> CreateAsync(CreateProjectDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(tenantId));

        var project = new Project
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectCode = dto.ProjectCode,
            ProjectName = dto.ProjectName,
            Description = dto.Description,
            ClientId = dto.ClientId,
            ProjectManagerId = dto.ProjectManagerId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Priority = dto.Priority,
            Budget = dto.Budget ?? 0,
            Notes = dto.Notes,
            Status = "Planning",
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var projectId = await _projectRepository.CreateAsync(project);
        return new ProjectCreateResult { ProjectId = projectId };
    }

    public async Task<bool> UpdateAsync(UpdateProjectDto dto, Guid tenantId, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(dto.Id, tenantId);
        if (project == null) return false;

        project.ProjectCode = dto.ProjectCode;
        project.ProjectName = dto.ProjectName;
        project.Description = dto.Description;
        project.ClientId = dto.ClientId;
        project.ProjectManagerId = dto.ProjectManagerId;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.ActualStartDate = dto.ActualStartDate;
        project.ActualEndDate = dto.ActualEndDate;
        // ERP-FIX-5: status only via Start/Complete/Pause/Cancel
        project.Priority = dto.Priority;
        project.Budget = dto.Budget ?? project.Budget;
        project.ActualCost = dto.ActualCost ?? project.ActualCost;
        project.Progress = dto.Progress ?? project.Progress;
        project.Notes = dto.Notes;
        project.UpdatedBy = userId.ToString();
        project.UpdatedAt = DateTime.UtcNow;

        return await _projectRepository.UpdateAsync(project);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        => await _projectRepository.DeleteAsync(id, tenantId);

    public async Task<bool> StartAsync(Guid id, Guid tenantId, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id, tenantId);
        if (project == null) return false;

        project.Status = "In Progress";
        project.ActualStartDate = DateTime.UtcNow;
        project.UpdatedBy = userId.ToString();
        project.UpdatedAt = DateTime.UtcNow;

        return await _projectRepository.UpdateAsync(project);
    }

    public async Task<bool> CompleteAsync(Guid id, CompleteProjectDto dto, Guid tenantId, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id, tenantId);
        if (project == null) return false;

        project.Status = "Completed";
        project.ActualEndDate = dto.CompletionDate;
        project.Notes = dto.Notes;
        project.UpdatedBy = userId.ToString();
        project.UpdatedAt = DateTime.UtcNow;

        return await _projectRepository.UpdateAsync(project);
    }

    public async Task<bool> PauseAsync(Guid id, PauseProjectDto dto, Guid tenantId, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id, tenantId);
        if (project == null) return false;

        project.Status = "Paused";
        project.Notes = dto.Notes;
        project.UpdatedBy = userId.ToString();
        project.UpdatedAt = DateTime.UtcNow;

        return await _projectRepository.UpdateAsync(project);
    }

    public async Task<bool> CancelAsync(Guid id, CancelProjectDto dto, Guid tenantId, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id, tenantId);
        if (project == null) return false;

        project.Status = "Cancelled";
        project.Notes = dto.Notes;
        project.UpdatedBy = userId.ToString();
        project.UpdatedAt = DateTime.UtcNow;

        return await _projectRepository.UpdateAsync(project);
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            ProjectCode = project.ProjectCode,
            ProjectName = project.ProjectName,
            Description = project.Description,
            ClientId = project.ClientId,
            ProjectManagerId = project.ProjectManagerId,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            ActualStartDate = project.ActualStartDate,
            ActualEndDate = project.ActualEndDate,
            Status = project.Status,
            Priority = project.Priority,
            Budget = project.Budget,
            ActualCost = project.ActualCost,
            Progress = project.Progress,
            Notes = project.Notes,
            ClientName = project.Client?.CustomerName ?? project.Client?.CompanyName,
            ProjectManagerName = $"{project.ProjectManager?.FirstName} {project.ProjectManager?.LastName}".Trim(),
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            CreatedBy = project.CreatedBy,
            UpdatedBy = project.UpdatedBy
        };
    }
}
