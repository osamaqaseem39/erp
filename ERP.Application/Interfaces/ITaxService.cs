using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces;

public interface ITaxService
{
    Task<TaxRateDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaxRateDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateTaxRateDto dto);
    Task<bool> UpdateAsync(UpdateTaxRateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<TaxRateDto>> GetActiveRatesAsync();
    Task<IEnumerable<TaxRateDto>> GetByTypeAsync(string taxType);
}