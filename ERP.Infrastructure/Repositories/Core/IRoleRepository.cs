using ERP.Domain.Core;

namespace ERP.Infrastructure.Repositories.Core;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<IEnumerable<Role>> GetActiveAsync();
    Task<Role?> GetByNameAsync(string name);
    Task<Guid> CreateAsync(Role role);
    Task<bool> UpdateAsync(Role role);
    Task<bool> DeleteAsync(Guid id);
}