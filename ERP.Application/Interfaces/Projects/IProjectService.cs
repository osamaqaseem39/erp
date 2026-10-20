using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Interfaces.Projects;

public interface IProjectService
{
    Task<ProjectDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProjectDto>> GetAllAsync(Guid tenantId, string? status = null, Guid? managerId = null, Guid? customerId = null);
    Task<IEnumerable<ProjectDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<ProjectDto>> GetByManagerAsync(Guid managerId, Guid tenantId);
    Task<IEnumerable<ProjectDto>> GetByClientAsync(Guid clientId, Guid tenantId);
    Task<ProjectCreateResult> CreateAsync(CreateProjectDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateProjectDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> StartAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, CompleteProjectDto dto, Guid tenantId, Guid userId);
    Task<bool> PauseAsync(Guid id, PauseProjectDto dto, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, CancelProjectDto dto, Guid tenantId, Guid userId);
}

public class ProjectCreateResult
{
    public Guid ProjectId { get; set; }
}
