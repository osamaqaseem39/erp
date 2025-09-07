using ERP.Domain.Core;

namespace ERP.Infrastructure.Repositories.Core;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id);
    Task<IEnumerable<Subscription>> GetAllAsync();
    Task<IEnumerable<Subscription>> GetByTenantAsync(Guid tenantId);
    Task<IEnumerable<Subscription>> GetByStatusAsync(string status);
    Task<Subscription?> GetActiveByTenantAsync(Guid tenantId);
    Task<Guid> CreateAsync(Subscription subscription);
    Task<bool> UpdateAsync(Subscription subscription);
    Task<bool> DeleteAsync(Guid id);
}