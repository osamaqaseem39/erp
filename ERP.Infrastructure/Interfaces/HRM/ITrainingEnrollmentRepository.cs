using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface ITrainingEnrollmentRepository
{
    Task<TrainingEnrollment?> GetByIdAsync(int id);
    Task<IEnumerable<TrainingEnrollment>> GetAllAsync();
    Task<TrainingEnrollment> AddAsync(TrainingEnrollment entity);
    Task UpdateAsync(TrainingEnrollment entity);
    Task DeleteAsync(int id);
}