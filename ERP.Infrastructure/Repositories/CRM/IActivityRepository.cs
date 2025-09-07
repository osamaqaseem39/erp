using ERP.Domain.CRM;

namespace ERP.Infrastructure.Repositories.CRM;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(Guid id);
    Task<IEnumerable<Activity>> GetAllAsync();
    Task<IEnumerable<Activity>> GetByTypeAsync(string activityType);
    Task<IEnumerable<Activity>> GetByEntityAsync(string entityType, Guid entityId);
    Task<Guid> CreateAsync(Activity activity);
    Task<bool> UpdateAsync(Activity activity);
    Task<bool> DeleteAsync(Guid id);
}