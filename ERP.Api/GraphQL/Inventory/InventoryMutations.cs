using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;
using HotChocolate;

namespace ERP.Api.GraphQL.Inventory;

[ExtendObjectType("Mutation")]
public class InventoryMutations
{
    // Stock Service Mutations
    public async Task<StockBalanceDto> AddStockBalance(
        [Service] IStockService service,
        StockBalanceDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create stock balance");
    }

    public async Task<StockBalanceDto> UpdateStockBalance(
        [Service] IStockService service,
        StockBalanceDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update stock balance");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("Stock balance not found");
    }

    public async Task<bool> DeleteStockBalance(
        [Service] IStockService service,
        Guid id)
    {
        return await service.DeleteAsync(id);
    }

    public async Task<bool> AdjustStock(
        [Service] IStockService service,
        StockAdjustmentDto input)
    {
        return await service.AdjustStockAsync(input);
    }

    // Stock Transfer Mutations
    public async Task<StockTransferDto> AddStockTransfer(
        [Service] IStockTransferService service,
        CreateStockTransferDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create stock transfer");
    }

    public async Task<StockTransferDto> UpdateStockTransfer(
        [Service] IStockTransferService service,
        UpdateStockTransferDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update stock transfer");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("Stock transfer not found");
    }

    public async Task<bool> DeleteStockTransfer(
        [Service] IStockTransferService service,
        Guid id)
    {
        return await service.DeleteAsync(id);
    }

    public async Task<bool> ApproveStockTransfer(
        [Service] IStockTransferService service,
        Guid id)
    {
        return await service.ApproveAsync(id);
    }

    public async Task<bool> CompleteStockTransfer(
        [Service] IStockTransferService service,
        Guid id)
    {
        return await service.CompleteAsync(id);
    }

    // Stock Adjustment Mutations
    public async Task<StockAdjustmentDto> AddStockAdjustment(
        [Service] IStockAdjustmentService service,
        CreateStockAdjustmentDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create stock adjustment");
    }

    public async Task<StockAdjustmentDto> UpdateStockAdjustment(
        [Service] IStockAdjustmentService service,
        UpdateStockAdjustmentDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update stock adjustment");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("Stock adjustment not found");
    }

    public async Task<bool> DeleteStockAdjustment(
        [Service] IStockAdjustmentService service,
        Guid id)
    {
        return await service.DeleteAsync(id);
    }

    // Item Category Mutations
    public async Task<ItemCategoryDto> AddItemCategory(
        [Service] IItemCategoryService service,
        CreateItemCategoryDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create item category");
    }

    public async Task<ItemCategoryDto> UpdateItemCategory(
        [Service] IItemCategoryService service,
        UpdateItemCategoryDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update item category");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("Item category not found");
    }

    public async Task<bool> DeleteItemCategory(
        [Service] IItemCategoryService service,
        Guid id)
    {
        return await service.DeleteAsync(id);
    }

    // Item Group Mutations
    public async Task<ItemGroupDto> AddItemGroup(
        [Service] IItemGroupService service,
        CreateItemGroupDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create item group");
    }

    public async Task<ItemGroupDto> UpdateItemGroup(
        [Service] IItemGroupService service,
        UpdateItemGroupDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update item group");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("Item group not found");
    }

    public async Task<bool> DeleteItemGroup(
        [Service] IItemGroupService service,
        Guid id)
    {
        return await service.DeleteAsync(id);
    }

    // Unit of Measure Mutations
    public async Task<UnitOfMeasureDto> AddUnitOfMeasure(
        [Service] IUnitOfMeasureService service,
        CreateUnitOfMeasureDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create unit of measure");
    }

    public async Task<UnitOfMeasureDto> UpdateUnitOfMeasure(
        [Service] IUnitOfMeasureService service,
        UpdateUnitOfMeasureDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update unit of measure");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("Unit of measure not found");
    }

    public async Task<bool> DeleteUnitOfMeasure(
        [Service] IUnitOfMeasureService service,
        Guid id)
    {
        return await service.DeleteAsync(id);
    }
}

