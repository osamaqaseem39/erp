using ERP.Domain.Projects;

namespace ERP.Infrastructure.Repositories.Projects;

public interface IProjectMilestoneRepository
{
    Task<ProjectMilestone?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProjectMilestone>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProjectMilestone>> GetByProjectAsync(Guid projectId, Guid tenantId);
    Task<IEnumerable<ProjectMilestone>> GetByStatusAsync(string status, Guid tenantId);
    Task<Guid> CreateAsync(ProjectMilestone projectMilestone);
    Task<bool> UpdateAsync(ProjectMilestone projectMilestone);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
