using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Mappers.Finance;

public class ExchangeRateMapper
{
    public ExchangeRateDto MapToDto(ExchangeRate source)
    {
        if (source == null) return null!;

        return new ExchangeRateDto
        {
            Id = source.ExchangeRateId,
            FromCurrency = source.FromCurrency,
            ToCurrency = source.ToCurrency,
            Rate = source.Rate,
            EffectiveDate = source.EffectiveDate,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt
        };
    }

    public ExchangeRate MapToEntity(CreateExchangeRateDto dto, Guid tenantId, Guid userId)
    {
        return new ExchangeRate
        {
            ExchangeRateId = Guid.NewGuid(),
            TenantId = tenantId,
            FromCurrency = dto.FromCurrency,
            ToCurrency = dto.ToCurrency,
            Rate = dto.Rate,
            EffectiveDate = dto.EffectiveDate,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MapToEntity(UpdateExchangeRateDto dto, ExchangeRate entity, Guid userId)
    {
        if (!string.IsNullOrEmpty(dto.FromCurrency))
            entity.FromCurrency = dto.FromCurrency;
        if (!string.IsNullOrEmpty(dto.ToCurrency))
            entity.ToCurrency = dto.ToCurrency;
        if (dto.Rate.HasValue)
            entity.Rate = dto.Rate.Value;
        if (dto.EffectiveDate.HasValue)
            entity.EffectiveDate = dto.EffectiveDate.Value;
    }
}

