using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces;

public interface ICurrencyService
{
    Task<CurrencyDto?> GetByIdAsync(string currencyCode);
    Task<IEnumerable<CurrencyDto>> GetAllAsync();
    Task<IEnumerable<CurrencyDto>> GetActiveAsync();
    Task<CurrencyDto> CreateAsync(CreateCurrencyDto dto);
    Task<CurrencyDto> UpdateAsync(UpdateCurrencyDto dto);
    Task<bool> DeleteAsync(string currencyCode);
    Task<bool> ExistsAsync(string currencyCode);
}