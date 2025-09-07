using ERP.Application.Interfaces;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Services;

public class TaxApiService : ITaxApiService
{
    private readonly ITaxService _service;

    public TaxApiService(ITaxService service)
    {
        _service = service;
    }

    public async Task<TaxRateDto?> GetByIdAsync(Guid id)
    {
        return await _service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<TaxRateDto>> GetAllAsync()
    {
        return await _service.GetAllAsync();
    }

    public async Task<Guid> CreateAsync(CreateTaxRateDto dto)
    {
        return await _service.CreateAsync(dto);
    }

    public async Task<bool> UpdateAsync(UpdateTaxRateDto dto)
    {
        return await _service.UpdateAsync(dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _service.DeleteAsync(id);
    }

    public async Task<IEnumerable<TaxRateDto>> GetActiveRatesAsync()
    {
        return await _service.GetActiveRatesAsync();
    }

    public async Task<IEnumerable<TaxRateDto>> GetByTypeAsync(string taxType)
    {
        return await _service.GetByTypeAsync(taxType);
    }
}