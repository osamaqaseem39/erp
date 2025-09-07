using ERP.Domain.Manufacturing;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public interface IWorkCenterRepository
{
    Task<WorkCenter?> GetByIdAsync(Guid id);
    Task<IEnumerable<WorkCenter>> GetAllAsync();
    Task<IEnumerable<WorkCenter>> GetActiveAsync();
    Task<IEnumerable<WorkCenter>> GetByTypeAsync(string workCenterType);
    Task<WorkCenter?> GetByCodeAsync(string workCenterCode);
    Task<Guid> CreateAsync(WorkCenter workCenter);
    Task<bool> UpdateAsync(WorkCenter workCenter);
    Task<bool> DeleteAsync(Guid id);
}