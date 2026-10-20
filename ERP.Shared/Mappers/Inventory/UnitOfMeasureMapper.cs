using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Shared.Mappers.Inventory;

public static class UnitOfMeasureMapper
{
    public static UnitOfMeasureDto ToDto(this UnitOfMeasure entity)
    {
        return new UnitOfMeasureDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Code = entity.UnitCode,
            Name = entity.UnitName,
            Description = entity.Description,
            ConversionFactor = entity.ConversionFactor ?? 0m,
            BaseUnitId = entity.BaseUnitId,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            BaseUnitName = entity.BaseUnit?.UnitName
        };
    }

    public static UnitOfMeasure ToEntity(this UnitOfMeasureDto dto)
    {
        return new UnitOfMeasure
        {
            Id = dto.Id,
            UnitCode = dto.Code,
            UnitName = dto.Name,
            Description = dto.Description,
            ConversionFactor = dto.ConversionFactor,
            BaseUnitId = dto.BaseUnitId,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}