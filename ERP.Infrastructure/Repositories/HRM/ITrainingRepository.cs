using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface ITrainingRepository
{
    Task<Training?> GetByIdAsync(Guid id);
    Task<IEnumerable<Training>> GetAllAsync();
    Task<IEnumerable<Training>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<Training>> GetByStatusAsync(string status);
    Task<IEnumerable<Training>> GetByTypeAsync(string trainingType);
    Task<Guid> CreateAsync(Training training);
    Task<bool> UpdateAsync(Training training);
    Task<bool> DeleteAsync(Guid id);
}