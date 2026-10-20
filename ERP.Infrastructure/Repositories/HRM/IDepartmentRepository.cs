using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Department>> GetAllAsync(Guid tenantId, Guid? companyId = null, Guid? parentDepartmentId = null);
    Task<IEnumerable<Department>> GetActiveAsync(Guid tenantId);
    Task<IEnumerable<Department>> GetByManagerAsync(Guid managerId, Guid tenantId);
    Task<Guid> CreateAsync(Department department);
    Task<bool> UpdateAsync(Department department);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> HasEmployeesAsync(Guid departmentId, Guid tenantId);
}
