using ERP.Domain.Projects;

namespace ERP.Infrastructure.Repositories.Projects;

public interface IProjectResourceRepository
{
    Task<ProjectResource?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProjectResource>> GetAllAsync();
    Task<IEnumerable<ProjectResource>> GetByProjectAsync(Guid projectId);
    Task<IEnumerable<ProjectResource>> GetByEmployeeAsync(Guid employeeId);
    Task<Guid> CreateAsync(ProjectResource projectResource);
    Task<bool> UpdateAsync(ProjectResource projectResource);
    Task<bool> DeleteAsync(Guid id);
}