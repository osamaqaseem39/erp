using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Infrastructure.Interfaces.Finance;
using ERP.Domain.Finance;

namespace ERP.Application.Finance;

public class TaxService : ITaxService
{
    private readonly ITaxRateRepository _repository;

    public TaxService(ITaxRateRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaxRateDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<TaxRateDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateTaxRateDto dto)
    {
        var entity = MapToEntity(dto);
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateTaxRateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<TaxRateDto>> GetActiveRatesAsync()
    {
        var entities = await _repository.GetActiveRatesAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<TaxRateDto>> GetByTypeAsync(string taxType)
    {
        var entities = await _repository.GetByTypeAsync(taxType);
        return entities.Select(MapToDto);
    }

    private static TaxRateDto MapToDto(TaxRate entity)
    {
        return new TaxRateDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            TaxCode = entity.Code,
            TaxName = entity.Name,
            TaxType = entity.Type,
            Rate = entity.Rate,
            IsActive = entity.IsActive,
            EffectiveFrom = entity.EffectiveDate,
            EffectiveTo = entity.ExpiryDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };
    }

    private static TaxRate MapToEntity(CreateTaxRateDto dto)
    {
        return new TaxRate
        {
            TenantId = dto.TenantId,
            Code = dto.TaxCode,
            Name = dto.TaxName,
            Type = dto.TaxType,
            Rate = dto.Rate,
            IsActive = dto.IsActive,
            EffectiveDate = dto.EffectiveFrom ?? DateTime.UtcNow,
            ExpiryDate = dto.EffectiveTo,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdateTaxRateDto dto, TaxRate entity)
    {
        entity.Code = dto.TaxCode;
        entity.Name = dto.TaxName;
        entity.Type = dto.TaxType;
        entity.Rate = dto.Rate;
        entity.IsActive = dto.IsActive;
        entity.EffectiveDate = dto.EffectiveFrom ?? DateTime.UtcNow;
        entity.ExpiryDate = dto.EffectiveTo;
        entity.UpdatedBy = dto.UpdatedBy;
    }
}