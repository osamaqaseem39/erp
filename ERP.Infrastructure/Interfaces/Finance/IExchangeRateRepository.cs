using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetByIdAsync(Guid id);
    Task<IEnumerable<ExchangeRate>> GetAllAsync();
    Task<ExchangeRate> CreateAsync(ExchangeRate exchangeRate);
    Task<ExchangeRate> UpdateAsync(ExchangeRate exchangeRate);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<ExchangeRate?> GetByCurrenciesAsync(Guid fromCurrencyId, Guid toCurrencyId);
}