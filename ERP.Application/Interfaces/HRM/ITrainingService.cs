using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces.HRM;

public interface ITrainingService
{
    Task<TrainingDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<TrainingDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateTrainingDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateTrainingDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<TrainingDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<TrainingDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<bool> EnrollEmployeeAsync(Guid trainingId, Guid employeeId, Guid tenantId, Guid userId);
    Task<bool> CompleteTrainingAsync(Guid trainingId, Guid employeeId, decimal score, Guid tenantId, Guid userId);
}
