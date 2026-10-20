using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface IDepartmentService
{
    Task<DepartmentDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<DepartmentDto>> GetAllAsync(Guid tenantId, Guid? companyId = null, Guid? parentDepartmentId = null);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, Guid tenantId, Guid userId);
    Task<DepartmentDto> UpdateAsync(UpdateDepartmentDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<DepartmentDto>> GetActiveAsync(Guid tenantId);
    Task<IEnumerable<DepartmentDto>> GetByManagerAsync(Guid managerId, Guid tenantId);
}
