using ERP.Shared.DTOs.Core;

namespace ERP.Application.Interfaces.Core;

public interface IPermissionService
{
    Task<IEnumerable<PermissionDto>> GetAllAsync(Guid tenantId, bool activeOnly = true);
    Task<IEnumerable<PermissionDto>> SeedDefaultPermissionsAsync(Guid tenantId, string createdBy);
    Task AssignAllPermissionsToRoleAsync(Guid roleId, Guid tenantId);
}
