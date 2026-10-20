using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IBenefitRepository
{
    Task<Benefit?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Benefit>> GetAllAsync(Guid tenantId, string? type = null, bool? isActive = null);
    Task<IEnumerable<Benefit>> GetActiveAsync(Guid tenantId);
    Task<IEnumerable<Benefit>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<Guid> CreateAsync(Benefit entity);
    Task<bool> UpdateAsync(Benefit entity);
    Task<bool> SoftDeleteAsync(Guid id, Guid tenantId);
    Task<EmployeeBenefitEnrollment?> GetActiveEnrollmentAsync(Guid benefitId, Guid employeeId, Guid tenantId);
    Task<Guid> CreateEnrollmentAsync(EmployeeBenefitEnrollment enrollment);
    Task<bool> UpdateEnrollmentAsync(EmployeeBenefitEnrollment enrollment);
}
