using ERP.Domain.Core;

namespace ERP.Infrastructure.Repositories.Core;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid id);
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<IEnumerable<Permission>> GetActiveAsync();
    Task<IEnumerable<Permission>> GetByModuleAsync(string module);
    Task<Permission?> GetByNameAsync(string name);
    Task<Guid> CreateAsync(Permission permission);
    Task<bool> UpdateAsync(Permission permission);
    Task<bool> DeleteAsync(Guid id);
}