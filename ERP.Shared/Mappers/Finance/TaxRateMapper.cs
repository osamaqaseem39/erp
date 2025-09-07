using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Finance;

public static class TaxRateMapper
{
    public static TaxRateDto ToDto(TaxRate entity)
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
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static TaxRate ToEntity(CreateTaxRateDto dto, Guid tenantId, string createdBy)
    {
        return new TaxRate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = dto.TaxName,
            Code = dto.TaxCode,
            Rate = dto.Rate,
            Type = dto.TaxType,
            EffectiveDate = dto.EffectiveFrom ?? DateTime.UtcNow,
            ExpiryDate = dto.EffectiveTo,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateTaxRateDto dto, TaxRate entity, string updatedBy)
    {
        entity.Name = dto.TaxName;
        entity.Code = dto.TaxCode;
        entity.Rate = dto.Rate;
        entity.Type = dto.TaxType;
        entity.IsActive = dto.IsActive;
        
        if (dto.EffectiveFrom.HasValue)
            entity.EffectiveDate = dto.EffectiveFrom.Value;
        
        entity.ExpiryDate = dto.EffectiveTo;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}