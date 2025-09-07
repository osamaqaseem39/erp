using ERP.Shared.DTOs.Core;

namespace ERP.Application.Interfaces.Core;

public interface IRoleService
{
    Task<RoleDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<RoleDto>> GetAllAsync();
    Task<IEnumerable<RoleDto>> GetActiveAsync();
    Task<RoleDto?> GetByNameAsync(string name);
    Task<Guid> CreateAsync(CreateRoleDto dto);
    Task<bool> UpdateAsync(UpdateRoleDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> AssignPermissionAsync(Guid roleId, Guid permissionId);
    Task<bool> RemovePermissionAsync(Guid roleId, Guid permissionId);
}