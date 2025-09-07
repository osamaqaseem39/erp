using ERP.Application.SCM.Interfaces;
using ERP.Domain.SCM;
using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;

    public ItemService(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task<ItemDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var item = await _itemRepository.GetByIdAsync(id, tenantId);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<IEnumerable<ItemDto>> GetAllAsync(Guid tenantId, Guid companyId)
    {
        var items = await _itemRepository.GetAllAsync(tenantId, companyId);
        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<ItemDto>> GetByCategoryAsync(Guid categoryId, Guid tenantId)
    {
        var items = await _itemRepository.GetByCategoryAsync(categoryId, tenantId);
        return items.Select(MapToDto);
    }

    public async Task<ItemDto> CreateAsync(CreateItemDto dto, Guid tenantId, string createdBy)
    {
        if (await _itemRepository.CodeExistsAsync(dto.ItemCode, tenantId, dto.CompanyId))
        {
            throw new InvalidOperationException($"Item with code '{dto.ItemCode}' already exists.");
        }

        var item = new Item
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            ItemCode = dto.ItemCode,
            ItemName = dto.ItemName,
            Description = dto.Description,
            CategoryId = Guid.Parse(dto.CategoryId),
            UnitOfMeasureId = dto.UnitOfMeasureId,
            ItemType = dto.ItemType,
            HSNCode = dto.HSNCode,
            BarCode = dto.BarCode,
            ReorderLevel = dto.ReorderLevel,
            ReorderQuantity = dto.ReorderQuantity,
            DefaultWarehouseId = dto.DefaultWarehouseId,
            CostPrice = dto.CostPrice,
            SellingPrice = dto.SellingPrice,
            IsSerialized = dto.IsSerialized,
            IsBatchTracked = dto.IsBatchTracked,
            CreatedBy = createdBy
        };

        var createdItem = await _itemRepository.AddAsync(item);
        return MapToDto(createdItem);
    }

    public async Task<ItemDto> UpdateAsync(UpdateItemDto dto, Guid tenantId, string updatedBy)
    {
        var item = await _itemRepository.GetByIdAsync(dto.Id, tenantId);
        if (item == null)
        {
            throw new InvalidOperationException("Item not found.");
        }

        if (await _itemRepository.CodeExistsAsync(dto.ItemCode, tenantId, item.CompanyId, dto.Id))
        {
            throw new InvalidOperationException($"Item with code '{dto.ItemCode}' already exists.");
        }

        item.ItemCode = dto.ItemCode;
        item.ItemName = dto.ItemName;
        item.Description = dto.Description;
        item.CategoryId = Guid.Parse(dto.CategoryId);
        item.UnitOfMeasureId = dto.UnitOfMeasureId;
        item.ItemType = dto.ItemType;
        item.HSNCode = dto.HSNCode;
        item.BarCode = dto.BarCode;
        item.ReorderLevel = dto.ReorderLevel;
        item.ReorderQuantity = dto.ReorderQuantity;
        item.DefaultWarehouseId = dto.DefaultWarehouseId ?? item.DefaultWarehouseId;
        item.CostPrice = dto.CostPrice;
        item.SellingPrice = dto.SellingPrice;
        item.IsSerialized = dto.IsSerialized;
        item.IsBatchTracked = dto.IsBatchTracked;
        item.IsActive = dto.IsActive;
        item.UpdatedBy = updatedBy;
        item.UpdatedAt = DateTime.UtcNow;

        var updatedItem = await _itemRepository.UpdateAsync(item);
        return MapToDto(updatedItem);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _itemRepository.DeleteAsync(id, tenantId);
    }

    public async Task<bool> ExistsAsync(Guid id, Guid tenantId)
    {
        return await _itemRepository.ExistsAsync(id, tenantId);
    }

    public async Task<IEnumerable<ItemDto>> GetLowStockItemsAsync(Guid tenantId, Guid companyId)
    {
        var items = await _itemRepository.GetLowStockItemsAsync(tenantId, companyId);
        return items.Select(MapToDto);
    }

    private static ItemDto MapToDto(Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            TenantId = item.TenantId,
            CompanyId = item.CompanyId,
            ItemCode = item.ItemCode,
            ItemName = item.ItemName,
            Description = item.Description,
            CategoryId = item.CategoryId.ToString(),
            CategoryName = item.Category?.CategoryName ?? string.Empty,
            UnitOfMeasureId = item.UnitOfMeasureId,
            UnitOfMeasureName = item.UnitOfMeasure?.UnitName ?? string.Empty,
            ItemType = item.ItemType,
            HSNCode = item.HSNCode,
            BarCode = item.BarCode,
            ReorderLevel = item.ReorderLevel,
            ReorderQuantity = item.ReorderQuantity,
            DefaultWarehouseId = item.DefaultWarehouseId,
            DefaultWarehouseName = item.DefaultWarehouse?.WarehouseName,
            CostPrice = item.CostPrice,
            SellingPrice = item.SellingPrice,
            IsSerialized = item.IsSerialized,
            IsBatchTracked = item.IsBatchTracked,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            CreatedBy = item.CreatedBy?.ToString(),
            UpdatedBy = item.UpdatedBy?.ToString()
        };
    }
}