using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface IJobOpeningRepository
{
    Task<JobOpening?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<JobOpening>> GetAllAsync(Guid tenantId);
    Task<JobOpening> CreateAsync(JobOpening entity);
    Task<JobOpening> UpdateAsync(JobOpening entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<JobOpening>> GetByDepartmentAsync(Guid departmentId, Guid tenantId);
    Task<IEnumerable<JobOpening>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<JobOpening>> GetActiveAsync(Guid tenantId);
}