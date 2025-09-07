using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IBatchNumberRepository
{
    Task<IEnumerable<BatchNumber>> GetByItemAsync(Guid itemId, Guid tenantId);
    Task<BatchNumber?> GetByIdAsync(Guid id, Guid tenantId);
    Task<BatchNumber> CreateAsync(BatchNumber entity);
    Task<BatchNumber> UpdateAsync(BatchNumber entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}