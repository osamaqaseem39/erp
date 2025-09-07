using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IHRSettingsRepository
{
    Task<HRSettings?> GetByIdAsync(Guid id);
    Task<IEnumerable<HRSettings>> GetAllAsync();
    Task<HRSettings?> GetByTenantAsync(Guid tenantId);
    Task<Guid> CreateAsync(HRSettings hrSettings);
    Task<bool> UpdateAsync(HRSettings hrSettings);
    Task<bool> DeleteAsync(Guid id);
}