using ERP.Domain.Projects;

namespace ERP.Infrastructure.Repositories.Projects;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetByStatusAsync(string status);
    Task<IEnumerable<Project>> GetByManagerAsync(Guid managerId);
    Task<IEnumerable<Project>> GetByClientAsync(Guid clientId);
    Task<Project?> GetByCodeAsync(string projectCode);
    Task<Guid> CreateAsync(Project project);
    Task<bool> UpdateAsync(Project project);
    Task<bool> DeleteAsync(Guid id);
}