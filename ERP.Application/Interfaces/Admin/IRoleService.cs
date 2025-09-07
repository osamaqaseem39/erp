using ERP.Shared.DTOs.Admin;
using PermissionDto = ERP.Shared.DTOs.Core.PermissionDto;
using RoleDto = ERP.Shared.DTOs.Admin.RoleDto;
using CreateRoleDto = ERP.Shared.DTOs.Admin.CreateRoleDto;
using UpdateRoleDto = ERP.Shared.DTOs.Admin.UpdateRoleDto;
using AssignPermissionsDto = ERP.Shared.DTOs.Admin.AssignPermissionsDto;

namespace ERP.Application.Interfaces.Admin;

public interface IRoleService
{
    Task<RoleDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<RoleDto>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<RoleDto> CreateAsync(CreateRoleDto dto, Guid tenantId, Guid userId);
    Task<RoleDto> UpdateAsync(UpdateRoleDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<RoleDto>> GetActiveAsync(Guid tenantId);
    Task<bool> AssignPermissionsAsync(Guid roleId, AssignPermissionsDto dto, Guid tenantId, Guid userId);
    Task<IEnumerable<PermissionDto>> GetPermissionsAsync(Guid roleId, Guid tenantId);
}