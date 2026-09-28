using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Mappers.Finance;

public class TaxRateMapper : BaseMapper<TaxRate, TaxRateDto>
{
    public override TaxRateDto Map(TaxRate source)
    {
        if (source == null) return null!;

        return new TaxRateDto
        {
            Id = source.Id,
            TaxCode = source.Code,
            TaxName = source.Name,
            TaxType = source.TaxType,
            Rate = source.Rate,
            IsActive = source.IsActive,
            EffectiveFrom = source.EffectiveDate,
            EffectiveTo = source.ExpiryDate,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override TaxRate Map(TaxRateDto destination)
    {
        if (destination == null) return null!;

        return new TaxRate
        {
            Id = destination.Id,
            Code = destination.TaxCode,
            Name = destination.TaxName,
            TaxType = destination.TaxType,
            Rate = destination.Rate,
            IsActive = destination.IsActive,
            EffectiveDate = destination.EffectiveFrom ?? DateTime.UtcNow,
            ExpiryDate = destination.EffectiveTo,
            TenantId = destination.TenantId != Guid.Empty ? destination.TenantId : Guid.Empty,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

