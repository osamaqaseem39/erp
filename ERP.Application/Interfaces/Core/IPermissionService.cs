using ERP.Shared.DTOs.Core;

namespace ERP.Application.Interfaces.Core;

public interface IPermissionService
{
    Task<PermissionDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PermissionDto>> GetAllAsync();
    Task<IEnumerable<PermissionDto>> GetActiveAsync();
    Task<IEnumerable<PermissionDto>> GetByModuleAsync(string module);
    Task<PermissionDto?> GetByNameAsync(string name);
    Task<Guid> CreateAsync(CreatePermissionDto dto);
    Task<bool> UpdateAsync(UpdatePermissionDto dto);
    Task<bool> DeleteAsync(Guid id);
}