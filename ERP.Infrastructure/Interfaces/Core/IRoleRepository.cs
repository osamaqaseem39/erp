using ERP.Domain.Core;

namespace ERP.Infrastructure.Interfaces.Core;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Role>> GetAllAsync(Guid tenantId);
    Task<Role> AddAsync(Role entity);
    Task UpdateAsync(Role entity);
    Task DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Role>> GetActiveAsync(Guid tenantId);
    Task AssignPermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, Guid tenantId);
}