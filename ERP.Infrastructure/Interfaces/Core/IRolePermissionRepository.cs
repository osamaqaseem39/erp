using ERP.Domain.Core;

namespace ERP.Infrastructure.Interfaces.Core;

public interface IRolePermissionRepository
{
    Task<RolePermission?> GetByIdAsync(int id);
    Task<IEnumerable<RolePermission>> GetAllAsync();
    Task<RolePermission> AddAsync(RolePermission entity);
    Task UpdateAsync(RolePermission entity);
    Task DeleteAsync(int id);
}