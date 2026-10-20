using ERP.Domain.Manufacturing;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public interface IWorkCenterRepository
{
    Task<WorkCenter?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<WorkCenter>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<WorkCenter>> GetActiveAsync(Guid tenantId);
    Task<IEnumerable<WorkCenter>> GetByTypeAsync(string workCenterType, Guid tenantId);
    Task<WorkCenter?> GetByCodeAsync(string workCenterCode, Guid tenantId);
    Task<Guid> CreateAsync(WorkCenter workCenter);
    Task<bool> UpdateAsync(WorkCenter workCenter);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
