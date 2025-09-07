using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Shared.Mappers.Inventory;

public static class ItemMapper
{
    public static ItemDto ToDto(Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            ItemCode = item.ItemCode,
            ItemName = item.ItemName,
            Description = item.Description,
            ItemCategoryId = item.CategoryId,
            CategoryId = item.CategoryId.ToString(),
            ItemType = item.ItemType,
            UnitOfMeasureId = item.UnitOfMeasureId,
            HSNCode = item.HSNCode,
            BarCode = item.BarCode,
            DefaultWarehouseId = item.DefaultWarehouseId,
            CostPrice = item.CostPrice,
            SellingPrice = item.SellingPrice,
            MinimumStock = 0,
            MaximumStock = 0,
            ReorderLevel = item.ReorderLevel,
            ReorderQuantity = item.ReorderQuantity,
            IsSerialized = item.IsSerialized,
            IsBatchTracked = item.IsBatchTracked,
            IsActive = item.IsActive,
            TenantId = item.TenantId,
            CreatedBy = item.CreatedBy,
            CreatedAt = item.CreatedAt,
            UpdatedBy = item.UpdatedBy,
            UpdatedAt = item.UpdatedAt
        };
    }

    public static Item ToEntity(CreateItemDto dto)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            ItemCode = dto.ItemCode,
            ItemName = dto.ItemName,
            Description = dto.Description,
            CategoryId = dto.ItemCategoryId,
            UnitOfMeasureId = dto.UnitOfMeasureId,
            ItemType = dto.ItemType,
            HSNCode = dto.HSNCode,
            BarCode = dto.BarCode,
            DefaultWarehouseId = dto.DefaultWarehouseId,
            CostPrice = dto.CostPrice,
            SellingPrice = dto.SellingPrice,
            ReorderLevel = dto.ReorderLevel,
            ReorderQuantity = dto.ReorderQuantity,
            IsSerialized = dto.IsSerialized,
            IsBatchTracked = dto.IsBatchTracked,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateEntity(UpdateItemDto dto, Item entity)
    {
        entity.ItemCode = dto.ItemCode;
        entity.ItemName = dto.ItemName;
        entity.Description = dto.Description;
        entity.CategoryId = dto.ItemCategoryId;
        entity.UnitOfMeasureId = dto.UnitOfMeasureId;
        entity.ItemType = dto.ItemType;
        entity.HSNCode = dto.HSNCode;
        entity.BarCode = dto.BarCode;
        entity.DefaultWarehouseId = dto.DefaultWarehouseId;
        entity.CostPrice = dto.CostPrice;
        entity.SellingPrice = dto.SellingPrice;
        entity.ReorderLevel = dto.ReorderLevel;
        entity.ReorderQuantity = dto.ReorderQuantity;
        entity.IsSerialized = dto.IsSerialized;
        entity.IsBatchTracked = dto.IsBatchTracked;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}