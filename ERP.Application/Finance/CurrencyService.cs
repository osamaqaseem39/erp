using ERP.Application.Interfaces;
using ERP.Domain.Finance;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _repository;

    public CurrencyService(ICurrencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<CurrencyDto?> GetByIdAsync(string currencyCode)
    {
        var entity = await _repository.GetByIdAsync(currencyCode);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<CurrencyDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<CurrencyDto>> GetActiveAsync()
    {
        var entities = await _repository.GetActiveAsync();
        return entities.Select(MapToDto);
    }

    public async Task<CurrencyDto> CreateAsync(CreateCurrencyDto dto)
    {
        var entity = new Currency
        {
            CurrencyCode = dto.CurrencyCode,
            CurrencyName = dto.CurrencyName,
            Iso4217 = dto.Iso4217,
            Symbol = dto.Symbol,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var createdEntity = await _repository.CreateAsync(entity);
        return MapToDto(createdEntity);
    }

    public async Task<CurrencyDto> UpdateAsync(UpdateCurrencyDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.CurrencyCode);
        if (entity == null)
            throw new InvalidOperationException("Currency not found");

        entity.CurrencyName = dto.CurrencyName ?? entity.CurrencyName;
        entity.Iso4217 = dto.Iso4217 ?? entity.Iso4217;
        entity.Symbol = dto.Symbol ?? entity.Symbol;
        entity.IsActive = dto.IsActive ?? entity.IsActive;

        var updatedEntity = await _repository.UpdateAsync(entity);
        return MapToDto(updatedEntity);
    }

    public async Task<bool> DeleteAsync(string currencyCode)
    {
        return await _repository.DeleteAsync(currencyCode);
    }

    public async Task<bool> ExistsAsync(string currencyCode)
    {
        return await _repository.ExistsAsync(currencyCode);
    }

    private static CurrencyDto MapToDto(Currency entity)
    {
        return new CurrencyDto
        {
            CurrencyCode = entity.CurrencyCode,
            CurrencyName = entity.CurrencyName,
            Iso4217 = entity.Iso4217,
            Symbol = entity.Symbol,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }
}