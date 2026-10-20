using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Shared.Mappers.Inventory;

public static class ItemVariantMapper
{
    public static ItemVariantDto ToDto(this ItemVariant entity)
    {
        return new ItemVariantDto
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            VariantCode = entity.VariantCode,
            VariantName = entity.VariantName,
            SKU = entity.SKU,
            CostPrice = entity.CostPrice,
            SellingPrice = entity.SellingPrice,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ItemName = entity.Item?.ItemName
        };
    }

    public static ItemVariant ToEntity(this ItemVariantDto dto)
    {
        return new ItemVariant
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            ItemId = dto.ItemId,
            VariantCode = dto.VariantCode,
            VariantName = dto.VariantName,
            SKU = dto.SKU,
            CostPrice = dto.CostPrice,
            SellingPrice = dto.SellingPrice,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}