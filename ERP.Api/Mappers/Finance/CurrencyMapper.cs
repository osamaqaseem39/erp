using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Mappers.Finance;

public class CurrencyMapper
{
    public CurrencyDto MapToDto(Currency source)
    {
        if (source == null) return null!;

        return new CurrencyDto
        {
            CurrencyCode = source.CurrencyCode,
            CurrencyName = source.CurrencyName,
            Iso4217 = source.Iso4217 ?? string.Empty,
            Symbol = source.Symbol ?? string.Empty,
            IsActive = source.IsActive
        };
    }

    public Currency MapToEntity(CreateCurrencyDto dto)
    {
        return new Currency
        {
            CurrencyCode = dto.CurrencyCode,
            CurrencyName = dto.CurrencyName,
            Iso4217 = dto.Iso4217,
            Symbol = dto.Symbol,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MapToEntity(UpdateCurrencyDto dto, Currency entity)
    {
        if (!string.IsNullOrEmpty(dto.CurrencyName))
            entity.CurrencyName = dto.CurrencyName;
        if (!string.IsNullOrEmpty(dto.Iso4217))
            entity.Iso4217 = dto.Iso4217;
        if (!string.IsNullOrEmpty(dto.Symbol))
            entity.Symbol = dto.Symbol;
        if (dto.IsActive.HasValue)
            entity.IsActive = dto.IsActive.Value;
    }
}

