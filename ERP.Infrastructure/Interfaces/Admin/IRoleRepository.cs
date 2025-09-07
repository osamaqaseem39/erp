using ERP.Domain.Core;

namespace ERP.Infrastructure.Interfaces.Admin;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Role>> GetAllAsync(Guid tenantId);
    Task<Role> CreateAsync(Role entity);
    Task<Role> UpdateAsync(Role entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<Role?> GetByNameAsync(string name, Guid tenantId);
    Task<IEnumerable<Role>> GetActiveAsync(Guid tenantId);
    Task<bool> AssignPermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, Guid tenantId);
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId, Guid tenantId);
}