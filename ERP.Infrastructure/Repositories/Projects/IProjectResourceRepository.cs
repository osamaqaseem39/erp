using ERP.Domain.Projects;

namespace ERP.Infrastructure.Repositories.Projects;

public interface IProjectResourceRepository
{
    Task<ProjectResource?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProjectResource>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProjectResource>> GetByProjectAsync(Guid projectId, Guid tenantId);
    Task<IEnumerable<ProjectResource>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<Guid> CreateAsync(ProjectResource projectResource);
    Task<bool> UpdateAsync(ProjectResource projectResource);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
