using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface ITaxCodeRepository
{
    Task<TaxCode?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<TaxCode>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<TaxCode>> GetActiveAsync(Guid tenantId);
    Task<IEnumerable<TaxCode>> GetByTypeAsync(string taxType, Guid tenantId);
    Task<TaxCode?> GetByCodeAsync(string taxCode, Guid tenantId);
    Task<Guid> CreateAsync(TaxCode taxCode);
    Task<bool> UpdateAsync(TaxCode taxCode);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
