using ERP.Domain.Projects;

namespace ERP.Infrastructure.Repositories.Projects;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Project>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<Project>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<Project>> GetByManagerAsync(Guid managerId, Guid tenantId);
    Task<IEnumerable<Project>> GetByClientAsync(Guid clientId, Guid tenantId);
    Task<Project?> GetByCodeAsync(string projectCode, Guid tenantId);
    Task<Guid> CreateAsync(Project project);
    Task<bool> UpdateAsync(Project project);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
