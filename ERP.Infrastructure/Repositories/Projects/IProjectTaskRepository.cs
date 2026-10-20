using ERP.Domain.Projects;

namespace ERP.Infrastructure.Repositories.Projects;

public interface IProjectTaskRepository
{
    Task<ProjectTask?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProjectTask>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProjectTask>> GetByProjectAsync(Guid projectId, Guid tenantId);
    Task<IEnumerable<ProjectTask>> GetByAssignedToAsync(Guid assignedToId, Guid tenantId);
    Task<IEnumerable<ProjectTask>> GetByStatusAsync(string status, Guid tenantId);
    Task<Guid> CreateAsync(ProjectTask projectTask);
    Task<bool> UpdateAsync(ProjectTask projectTask);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
