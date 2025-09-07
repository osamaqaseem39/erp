using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces;

public interface IExchangeRateService
{
    Task<ExchangeRateDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ExchangeRateDto>> GetAllAsync();
    Task<IEnumerable<ExchangeRateDto>> GetByCurrencyAsync(string fromCurrency, string toCurrency);
    Task<ExchangeRateDto?> GetCurrentRateAsync(string fromCurrency, string toCurrency);
    Task<ExchangeRateDto> CreateAsync(CreateExchangeRateDto dto);
    Task<ExchangeRateDto> UpdateAsync(UpdateExchangeRateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency, DateTime? asOfDate = null);
}