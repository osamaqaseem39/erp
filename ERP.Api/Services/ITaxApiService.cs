using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Services;

public interface ITaxApiService : IApiService<TaxRateDto, CreateTaxRateDto, UpdateTaxRateDto>
{
    Task<IEnumerable<TaxRateDto>> GetActiveRatesAsync();
    Task<IEnumerable<TaxRateDto>> GetByTypeAsync(string taxType);
}