using ERP.Application.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;
using System;
using System.Linq;

namespace ERP.Application.HRM;

/// <summary>DI stub for IDepartmentService. Replace with real business logic incrementally.</summary>
public class DepartmentService : IDepartmentService
{
    public Task<DepartmentDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return Task.FromResult<DepartmentDto?>(null);
    }

    public Task<IEnumerable<DepartmentDto>> GetAllAsync(Guid tenantId, Guid? companyId = null)
    {
        return Task.FromResult(Enumerable.Empty<DepartmentDto>());
    }

    public Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, Guid tenantId, Guid userId)
    {
        throw new NotImplementedException("DepartmentService.CreateAsync is not yet implemented");
    }

    public Task<DepartmentDto> UpdateAsync(UpdateDepartmentDto dto, Guid tenantId, Guid userId)
    {
        throw new NotImplementedException("DepartmentService.UpdateAsync is not yet implemented");
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<DepartmentDto>> GetActiveAsync(Guid tenantId)
    {
        return Task.FromResult(Enumerable.Empty<DepartmentDto>());
    }

    public Task<IEnumerable<DepartmentDto>> GetByManagerAsync(Guid managerId, Guid tenantId)
    {
        return Task.FromResult(Enumerable.Empty<DepartmentDto>());
    }

}
