using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;
using HotChocolate;

namespace ERP.Api.GraphQL.Inventory;

[ExtendObjectType("Query")]
public class InventoryQueries
{
    // Stock Service Queries
    public async Task<StockBalanceDto?> GetStockBalance(
        [Service] IStockService service,
        Guid id)
    {
        return await service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<StockBalanceDto>> GetStockBalances(
        [Service] IStockService service,
        Guid? itemId = null,
        Guid? warehouseId = null)
    {
        if (itemId.HasValue && warehouseId.HasValue)
            return await service.GetByItemAndWarehouseAsync(itemId.Value, warehouseId.Value);
        if (itemId.HasValue)
            return await service.GetByItemAsync(itemId.Value);
        if (warehouseId.HasValue)
            return await service.GetByWarehouseAsync(warehouseId.Value);
        return await service.GetAllAsync();
    }

    public async Task<decimal> GetStockLevel(
        [Service] IStockService service,
        Guid itemId,
        Guid warehouseId)
    {
        return await service.GetStockLevelAsync(itemId, warehouseId);
    }

    // Stock Transfer Queries
    public async Task<StockTransferDto?> GetStockTransfer(
        [Service] IStockTransferService service,
        Guid id)
    {
        return await service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<StockTransferDto>> GetStockTransfers(
        [Service] IStockTransferService service,
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        string? status = null)
    {
        if (fromWarehouseId.HasValue)
            return await service.GetByFromWarehouseAsync(fromWarehouseId.Value);
        if (toWarehouseId.HasValue)
            return await service.GetByToWarehouseAsync(toWarehouseId.Value);
        if (!string.IsNullOrEmpty(status))
            return await service.GetByStatusAsync(status);
        return await service.GetAllAsync();
    }

    // Stock Adjustment Queries
    public async Task<StockAdjustmentDto?> GetStockAdjustment(
        [Service] IStockAdjustmentService service,
        Guid id)
    {
        return await service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<StockAdjustmentDto>> GetStockAdjustments(
        [Service] IStockAdjustmentService service,
        Guid? itemId = null,
        Guid? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        if (itemId.HasValue)
            return await service.GetByItemAsync(itemId.Value);
        if (warehouseId.HasValue)
            return await service.GetByWarehouseAsync(warehouseId.Value);
        if (fromDate.HasValue && toDate.HasValue)
            return await service.GetByDateRangeAsync(fromDate.Value, toDate.Value);
        return await service.GetAllAsync();
    }

    // Item Category Queries
    public async Task<ItemCategoryDto?> GetItemCategory(
        [Service] IItemCategoryService service,
        Guid id)
    {
        return await service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<ItemCategoryDto>> GetItemCategories(
        [Service] IItemCategoryService service)
    {
        return await service.GetAllAsync();
    }

    // Item Group Queries
    public async Task<ItemGroupDto?> GetItemGroup(
        [Service] IItemGroupService service,
        Guid id)
    {
        return await service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<ItemGroupDto>> GetItemGroups(
        [Service] IItemGroupService service)
    {
        return await service.GetAllAsync();
    }

    // Unit of Measure Queries
    public async Task<UnitOfMeasureDto?> GetUnitOfMeasure(
        [Service] IUnitOfMeasureService service,
        Guid id)
    {
        return await service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<UnitOfMeasureDto>> GetUnitsOfMeasure(
        [Service] IUnitOfMeasureService service)
    {
        return await service.GetAllAsync();
    }
}

