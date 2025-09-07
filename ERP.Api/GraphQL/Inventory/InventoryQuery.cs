using ERP.Application.SCM.Interfaces;
using ERP.Shared.DTOs.Inventory;
using HotChocolate;
using HotChocolate.Types;

namespace ERP.Api.GraphQL.Inventory;

[ExtendObjectType("Query")]
public class InventoryQuery
{
    public async Task<WarehouseDto?> GetWarehouseById(
        [Service] IWarehouseService warehouseService,
        [ID] Guid id,
        [ID] Guid tenantId)
    {
        return await warehouseService.GetByIdAsync(id, tenantId);
    }

    public async Task<IEnumerable<WarehouseDto>> GetWarehouses(
        [Service] IWarehouseService warehouseService,
        [ID] Guid tenantId,
        [ID] Guid companyId)
    {
        return await warehouseService.GetAllAsync(tenantId, companyId);
    }

    public async Task<ItemDto?> GetItemById(
        [Service] IItemService itemService,
        [ID] Guid id,
        [ID] Guid tenantId)
    {
        return await itemService.GetByIdAsync(id, tenantId);
    }

    public async Task<IEnumerable<ItemDto>> GetItems(
        [Service] IItemService itemService,
        [ID] Guid tenantId,
        [ID] Guid companyId,
        [ID] Guid? categoryId = null)
    {
        if (categoryId.HasValue)
            return await itemService.GetByCategoryAsync(categoryId.Value, tenantId);
        else
            return await itemService.GetAllAsync(tenantId, companyId);
    }

    public async Task<IEnumerable<ItemDto>> GetLowStockItems(
        [Service] IItemService itemService,
        [ID] Guid tenantId,
        [ID] Guid companyId)
    {
        return await itemService.GetLowStockItemsAsync(tenantId, companyId);
    }

    public async Task<StockBalanceDto?> GetStockBalance(
        [Service] IInventoryService inventoryService,
        [ID] Guid itemId,
        [ID] Guid warehouseId,
        [ID] Guid tenantId)
    {
        return await inventoryService.GetStockBalanceAsync(itemId, warehouseId, tenantId);
    }

    public async Task<IEnumerable<StockBalanceDto>> GetStockBalances(
        [Service] IInventoryService inventoryService,
        [ID] Guid tenantId,
        [ID] Guid companyId,
        [ID] Guid? warehouseId = null)
    {
        return await inventoryService.GetAllStockBalancesAsync(tenantId, companyId, warehouseId);
    }

    public async Task<IEnumerable<StockLedgerDto>> GetStockLedger(
        [Service] IInventoryService inventoryService,
        [ID] Guid itemId,
        [ID] Guid warehouseId,
        [ID] Guid tenantId,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        return await inventoryService.GetStockLedgerAsync(itemId, warehouseId, tenantId, fromDate, toDate);
    }

    public async Task<IEnumerable<StockTransferDto>> GetStockTransfers(
        [Service] IInventoryService inventoryService,
        [ID] Guid tenantId,
        [ID] Guid companyId)
    {
        return await inventoryService.GetStockTransfersAsync(tenantId, companyId);
    }

    public async Task<StockTransferDto?> GetStockTransferById(
        [Service] IInventoryService inventoryService,
        [ID] Guid id,
        [ID] Guid tenantId)
    {
        return await inventoryService.GetStockTransferByIdAsync(id, tenantId);
    }
}