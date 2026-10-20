using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface ITrainingRepository
{
    Task<Training?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Training>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<Training>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<Training>> GetByStatusAsync(string status, Guid tenantId);
    Task<Guid> CreateAsync(Training training);
    Task<bool> UpdateAsync(Training training);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<TrainingEnrollment?> GetEnrollmentAsync(Guid trainingId, Guid employeeId, Guid tenantId);
    Task<Guid> CreateEnrollmentAsync(TrainingEnrollment enrollment);
    Task<bool> UpdateEnrollmentAsync(TrainingEnrollment enrollment);
    Task<int> CountActiveEnrollmentsAsync(Guid trainingId, Guid tenantId);
}
