using ERP.Domain.HRM;

namespace ERP.Infrastructure.Interfaces.HRM;

public interface ITrainingRepository
{
    Task<Training?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Training>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(Training entity);
    Task<bool> UpdateAsync(Training entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Training>> GetByEmployeeAsync(Guid employeeId, Guid tenantId);
    Task<IEnumerable<Training>> GetByStatusAsync(string status, Guid tenantId);
}
