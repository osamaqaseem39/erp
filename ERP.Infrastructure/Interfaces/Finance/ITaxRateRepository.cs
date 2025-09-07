using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface ITaxRateRepository
{
    Task<TaxRate?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaxRate>> GetAllAsync();
    Task<Guid> CreateAsync(TaxRate entity);
    Task<bool> UpdateAsync(TaxRate entity);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<TaxRate>> GetActiveRatesAsync();
    Task<IEnumerable<TaxRate>> GetByTypeAsync(string taxType);
}