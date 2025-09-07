using ERP.Domain.Projects;

namespace ERP.Infrastructure.Repositories.Projects;

public interface IProjectTaskRepository
{
    Task<ProjectTask?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProjectTask>> GetAllAsync();
    Task<IEnumerable<ProjectTask>> GetByProjectAsync(Guid projectId);
    Task<IEnumerable<ProjectTask>> GetByAssignedToAsync(Guid assignedToId);
    Task<IEnumerable<ProjectTask>> GetByStatusAsync(string status);
    Task<Guid> CreateAsync(ProjectTask projectTask);
    Task<bool> UpdateAsync(ProjectTask projectTask);
    Task<bool> DeleteAsync(Guid id);
}