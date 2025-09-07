using ERP.Domain.Projects;

namespace ERP.Infrastructure.Repositories.Projects;

public interface IProjectMilestoneRepository
{
    Task<ProjectMilestone?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProjectMilestone>> GetAllAsync();
    Task<IEnumerable<ProjectMilestone>> GetByProjectAsync(Guid projectId);
    Task<IEnumerable<ProjectMilestone>> GetByStatusAsync(string status);
    Task<Guid> CreateAsync(ProjectMilestone projectMilestone);
    Task<bool> UpdateAsync(ProjectMilestone projectMilestone);
    Task<bool> DeleteAsync(Guid id);
}