using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Interfaces.Projects;

public interface IProjectTaskService
{
    Task<ProjectTaskDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProjectTaskDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProjectTaskDto>> GetByProjectAsync(Guid projectId, Guid tenantId);
    Task<IEnumerable<ProjectTaskDto>> GetByAssignedToAsync(Guid assignedToId, Guid tenantId);
    Task<IEnumerable<ProjectTaskDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<Guid> CreateAsync(CreateProjectTaskDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateProjectTaskDto dto, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
