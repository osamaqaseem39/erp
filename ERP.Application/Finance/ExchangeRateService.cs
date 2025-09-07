using ERP.Application.Interfaces;
using ERP.Domain.Finance;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _repository;

    public ExchangeRateService(IExchangeRateRepository repository)
    {
        _repository = repository;
    }

    public async Task<ExchangeRateDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<ExchangeRateDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ExchangeRateDto>> GetByCurrencyAsync(string fromCurrency, string toCurrency)
    {
        var entities = await _repository.GetByCurrencyAsync(fromCurrency, toCurrency);
        return entities.Select(MapToDto);
    }

    public async Task<ExchangeRateDto?> GetCurrentRateAsync(string fromCurrency, string toCurrency)
    {
        var entity = await _repository.GetCurrentRateAsync(fromCurrency, toCurrency);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<ExchangeRateDto> CreateAsync(CreateExchangeRateDto dto)
    {
        var entity = new ExchangeRate
        {
            ExchangeRateId = Guid.NewGuid(),
            TenantId = dto.TenantId,
            FromCurrency = dto.FromCurrency,
            ToCurrency = dto.ToCurrency,
            Rate = dto.Rate,
            EffectiveDate = dto.EffectiveDate,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = !string.IsNullOrEmpty(dto.CreatedBy) ? Guid.Parse(dto.CreatedBy) : null
        };

        var createdEntity = await _repository.CreateAsync(entity);
        return MapToDto(createdEntity);
    }

    public async Task<ExchangeRateDto> UpdateAsync(UpdateExchangeRateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null)
            throw new InvalidOperationException("Exchange rate not found");

        if (dto.FromCurrency != null) entity.FromCurrency = dto.FromCurrency;
        if (dto.ToCurrency != null) entity.ToCurrency = dto.ToCurrency;
        if (dto.Rate.HasValue) entity.Rate = dto.Rate.Value;
        if (dto.EffectiveDate.HasValue) entity.EffectiveDate = dto.EffectiveDate.Value;

        var updatedEntity = await _repository.UpdateAsync(entity);
        return MapToDto(updatedEntity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency, DateTime? asOfDate = null)
    {
        if (fromCurrency == toCurrency)
            return amount;

        var rate = await GetCurrentRateAsync(fromCurrency, toCurrency);
        if (rate == null)
            throw new InvalidOperationException($"Exchange rate not found for {fromCurrency} to {toCurrency}");

        return amount * rate.Rate;
    }

    private static ExchangeRateDto MapToDto(ExchangeRate entity)
    {
        return new ExchangeRateDto
        {
            Id = entity.ExchangeRateId,
            TenantId = entity.TenantId,
            FromCurrency = entity.FromCurrency,
            ToCurrency = entity.ToCurrency,
            Rate = entity.Rate,
            EffectiveDate = entity.EffectiveDate,
            FromCurrencyName = entity.FromCurrencyNavigation?.CurrencyName,
            ToCurrencyName = entity.ToCurrencyNavigation?.CurrencyName,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy?.ToString()
        };
    }
}