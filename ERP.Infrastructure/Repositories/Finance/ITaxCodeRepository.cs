using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface ITaxCodeRepository
{
    Task<TaxCode?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaxCode>> GetAllAsync();
    Task<IEnumerable<TaxCode>> GetActiveAsync();
    Task<TaxCode?> GetByCodeAsync(string taxCode);
    Task<Guid> CreateAsync(TaxCode taxCode);
    Task<bool> UpdateAsync(TaxCode taxCode);
    Task<bool> DeleteAsync(Guid id);
}