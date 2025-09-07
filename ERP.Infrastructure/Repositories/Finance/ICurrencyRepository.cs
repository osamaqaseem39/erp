using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface ICurrencyRepository
{
    Task<Currency?> GetByIdAsync(string currencyCode);
    Task<IEnumerable<Currency>> GetAllAsync();
    Task<IEnumerable<Currency>> GetActiveAsync();
    Task<Currency> CreateAsync(Currency entity);
    Task<Currency> UpdateAsync(Currency entity);
    Task<bool> DeleteAsync(string currencyCode);
    Task<bool> ExistsAsync(string currencyCode);
}