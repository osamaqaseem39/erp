using ERP.Application.SCM.Interfaces;
using ERP.Shared.DTOs.Inventory;
using HotChocolate;
using HotChocolate.Types;

namespace ERP.Api.GraphQL.Inventory;

[ExtendObjectType("Mutation")]
public class InventoryMutation
{
    public async Task<WarehouseDto> CreateWarehouse(
        [Service] IWarehouseService warehouseService,
        CreateWarehouseDto input,
        [ID] Guid tenantId,
        string createdBy)
    {
        return await warehouseService.CreateAsync(input, tenantId, createdBy);
    }

    public async Task<WarehouseDto> UpdateWarehouse(
        [Service] IWarehouseService warehouseService,
        UpdateWarehouseDto input,
        [ID] Guid tenantId,
        string updatedBy)
    {
        return await warehouseService.UpdateAsync(input, tenantId, updatedBy);
    }

    public async Task<bool> DeleteWarehouse(
        [Service] IWarehouseService warehouseService,
        [ID] Guid id,
        [ID] Guid tenantId)
    {
        return await warehouseService.DeleteAsync(id, tenantId);
    }

    public async Task<ItemDto> CreateItem(
        [Service] IItemService itemService,
        CreateItemDto input,
        [ID] Guid tenantId,
        string createdBy)
    {
        return await itemService.CreateAsync(input, tenantId, createdBy);
    }

    public async Task<ItemDto> UpdateItem(
        [Service] IItemService itemService,
        UpdateItemDto input,
        [ID] Guid tenantId,
        string updatedBy)
    {
        return await itemService.UpdateAsync(input, tenantId, updatedBy);
    }

    public async Task<bool> DeleteItem(
        [Service] IItemService itemService,
        [ID] Guid id,
        [ID] Guid tenantId)
    {
        return await itemService.DeleteAsync(id, tenantId);
    }

    public async Task<bool> AdjustInventory(
        [Service] IInventoryService inventoryService,
        AdjustInventoryInput input,
        [ID] Guid tenantId,
        string createdBy)
    {
        return await inventoryService.AdjustInventoryAsync(
            input.ItemId,
            input.WarehouseId,
            tenantId,
            input.AdjustmentType,
            input.Quantity,
            input.Reason,
            input.ApprovedBy,
            createdBy);
    }

    public async Task<bool> TransferStock(
        [Service] IInventoryService inventoryService,
        CreateStockTransferDto input,
        [ID] Guid tenantId,
        string createdBy)
    {
        return await inventoryService.TransferStockAsync(input, tenantId, createdBy);
    }

    public async Task<bool> UpdateTransferStatus(
        [Service] IInventoryService inventoryService,
        [ID] Guid id,
        [ID] Guid tenantId,
        string status,
        string? approvedBy,
        string updatedBy)
    {
        return await inventoryService.UpdateTransferStatusAsync(id, tenantId, status, approvedBy, updatedBy);
    }
}

public class AdjustInventoryInput
{
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public string AdjustmentType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
}