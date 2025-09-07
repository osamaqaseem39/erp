using ERP.Shared.DTOs.HRM;

namespace ERP.Application.Interfaces;

public interface ITrainingService
{
    Task<TrainingDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TrainingDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateTrainingDto dto);
    Task<bool> UpdateAsync(UpdateTrainingDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<TrainingDto>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<TrainingDto>> GetByStatusAsync(string status);
    Task<bool> EnrollEmployeeAsync(Guid trainingId, Guid employeeId);
    Task<bool> CompleteTrainingAsync(Guid trainingId, Guid employeeId, decimal score);
}