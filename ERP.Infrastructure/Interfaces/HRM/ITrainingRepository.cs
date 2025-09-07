using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface ITrainingRepository
{
    Task<Training?> GetByIdAsync(Guid id);
    Task<IEnumerable<Training>> GetAllAsync();
    Task<Guid> CreateAsync(Training entity);
    Task<bool> UpdateAsync(Training entity);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Training>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<Training>> GetByStatusAsync(string status);
}