using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Shared.Mappers.Inventory;

public class ItemPricingMapper : IMapper<ItemPricing, ItemPricingDto>
{
    public ItemPricingDto ToDto(ItemPricing entity)
    {
        return new ItemPricingDto
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            PriceListName = entity.PriceListName,
            UnitPrice = entity.UnitPrice,
            MinQuantity = entity.MinQuantity,
            MaxQuantity = entity.MaxQuantity ?? 0m,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            ItemName = entity.Item?.ItemName
        };
    }

    public ItemPricing ToEntity(ItemPricingDto dto)
    {
        return new ItemPricing
        {
            Id = dto.Id,
            ItemId = dto.ItemId,
            PriceListName = dto.PriceListName,
            UnitPrice = dto.UnitPrice,
            MinQuantity = dto.MinQuantity,
            MaxQuantity = dto.MaxQuantity,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            CreatedBy = dto.CreatedBy,
            UpdatedBy = dto.UpdatedBy
        };
    }
}