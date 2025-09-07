using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetByIdAsync(Guid id);
    Task<IEnumerable<ExchangeRate>> GetAllAsync();
    Task<IEnumerable<ExchangeRate>> GetByCurrencyAsync(string fromCurrency, string toCurrency);
    Task<ExchangeRate?> GetCurrentRateAsync(string fromCurrency, string toCurrency);
    Task<ExchangeRate> CreateAsync(ExchangeRate entity);
    Task<ExchangeRate> UpdateAsync(ExchangeRate entity);
    Task<bool> DeleteAsync(Guid id);
}