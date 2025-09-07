using ERP.Domain.Core;

namespace ERP.Infrastructure.Repositories.Core;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<IEnumerable<Tenant>> GetActiveAsync();
    Task<Tenant?> GetByDomainAsync(string domain);
    Task<Guid> CreateAsync(Tenant tenant);
    Task<bool> UpdateAsync(Tenant tenant);
    Task<bool> DeleteAsync(Guid id);
}