using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;
using HotChocolate;
using ScmBarcode = ERP.Application.SCM.Interfaces.IBarcodeService;
using ScmBatchNumber = ERP.Application.SCM.Interfaces.IBatchNumberService;
using ScmItemAttribute = ERP.Application.SCM.Interfaces.IItemAttributeService;
using ScmValuationMethod = ERP.Application.SCM.Interfaces.IInventoryValuationMethodService;
using ScmItemGroup = ERP.Application.SCM.Interfaces.IItemGroupService;
using ScmItemPricing = ERP.Application.SCM.Interfaces.IItemPricingService;
using ScmItemVariant = ERP.Application.SCM.Interfaces.IItemVariantService;
using ScmReorderRule = ERP.Application.SCM.Interfaces.IReorderRuleService;
using ScmSerialNumber = ERP.Application.SCM.Interfaces.ISerialNumberService;
using ScmUnitOfMeasure = ERP.Application.SCM.Interfaces.IUnitOfMeasureService;
using ScmWarehouse = ERP.Application.SCM.Interfaces.IWarehouseService;
using ScmItem = ERP.Application.SCM.Interfaces.IItemService;
using ScmInventory = ERP.Application.SCM.Interfaces.IInventoryService;
using AppStockService = ERP.Application.Interfaces.IStockService;

namespace ERP.Api.GraphQL.Inventory;

[ExtendObjectType("Query")]
public class InventoryQueries
{
    // Stock balance queries (tenant-scoped Application.IStockService)
    public async Task<StockBalanceDto?> GetStockBalance(
        [Service] AppStockService service,
        Guid id,
        Guid tenantId)
        => await service.GetByIdAsync(id, tenantId);

    public async Task<IEnumerable<StockBalanceDto>> GetStockBalances(
        [Service] AppStockService service,
        Guid tenantId,
        Guid? itemId = null,
        Guid? warehouseId = null)
    {
        if (itemId.HasValue && warehouseId.HasValue)
        {
            var one = await service.GetByItemAndWarehouseAsync(itemId.Value, warehouseId.Value, tenantId);
            return one == null ? [] : [one];
        }
        if (itemId.HasValue)
            return await service.GetByItemAsync(itemId.Value, tenantId);
        if (warehouseId.HasValue)
            return await service.GetByWarehouseAsync(warehouseId.Value, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    public async Task<decimal> GetStockLevel(
        [Service] AppStockService service,
        Guid itemId,
        Guid warehouseId,
        Guid tenantId)
        => await service.GetStockLevelAsync(itemId, warehouseId, tenantId);

    public async Task<IEnumerable<StockLedgerDto>> GetStockLedger(
        [Service] AppStockService service,
        Guid itemId,
        Guid tenantId,
        Guid? warehouseId = null,
        DateTime? toDate = null)
    {
        if (itemId == Guid.Empty || tenantId == Guid.Empty)
            return [];
        return await service.GetStockLedgerAsync(itemId, tenantId, warehouseId, toDate);
    }

    public async Task<StockAverageRateDto> GetStockAverageRate(
        [Service] AppStockService service,
        Guid tenantId,
        Guid itemId,
        Guid? warehouseId = null,
        DateTime? asOfDate = null,
        bool updateItemCostPrice = false)
    {
        if (itemId == Guid.Empty || tenantId == Guid.Empty)
            return new StockAverageRateDto { ItemId = itemId, WarehouseId = warehouseId, AsOfDate = asOfDate };
        return await service.CalculateAverageRateAsync(tenantId, new CalculateStockAverageRateRequest
        {
            ItemId = itemId,
            WarehouseId = warehouseId,
            AsOfDate = asOfDate,
            UpdateItemCostPrice = updateItemCostPrice
        });
    }

    public async Task<StockAverageRateDto> CalculateStockAverageRate(
        [Service] AppStockService service,
        CalculateStockAverageRateRequest input,
        Guid tenantId)
    {
        if (input.ItemId == Guid.Empty || tenantId == Guid.Empty)
            return new StockAverageRateDto
            {
                ItemId = input.ItemId,
                WarehouseId = input.WarehouseId,
                AsOfDate = input.AsOfDate
            };
        return await service.CalculateAverageRateAsync(tenantId, input);
    }

    public async Task<InventoryValuationDto> GetInventoryValuation(
        [Service] ScmInventory service,
        Guid tenantId,
        Guid? warehouseId = null,
        DateTime? asOfDate = null)
        => await service.GetInventoryValuationAsync(tenantId, warehouseId, asOfDate);

    // Stock Transfer Queries
    public async Task<StockTransferDto?> GetStockTransfer(
        [Service] IStockTransferService service,
        Guid id,
        Guid tenantId)
    {
        return await service.GetByIdAsync(id, tenantId);
    }

    public async Task<IEnumerable<StockTransferDto>> GetStockTransfers(
        [Service] IStockTransferService service,
        Guid tenantId,
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        string? status = null)
    {
        if (fromWarehouseId.HasValue)
            return await service.GetByFromWarehouseAsync(fromWarehouseId.Value, tenantId);
        if (toWarehouseId.HasValue)
            return await service.GetByToWarehouseAsync(toWarehouseId.Value, tenantId);
        if (!string.IsNullOrEmpty(status))
            return await service.GetByStatusAsync(status, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    // Stock Adjustment Queries
    public async Task<StockAdjustmentDto?> GetStockAdjustment(
        [Service] IStockAdjustmentService service,
        Guid id,
        Guid tenantId)
    {
        return await service.GetByIdAsync(id, tenantId);
    }

    public async Task<IEnumerable<StockAdjustmentDto>> GetStockAdjustments(
        [Service] IStockAdjustmentService service,
        Guid tenantId,
        Guid? itemId = null,
        Guid? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        if (itemId.HasValue)
            return await service.GetByItemAsync(itemId.Value, tenantId);
        if (warehouseId.HasValue)
            return await service.GetByWarehouseAsync(warehouseId.Value, tenantId);
        if (fromDate.HasValue && toDate.HasValue)
            return await service.GetByDateRangeAsync(fromDate.Value, toDate.Value, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    // Item Category Queries
    public async Task<ItemCategoryDto?> GetItemCategory(
        [Service] IItemCategoryService service,
        Guid id,
        Guid tenantId)
    {
        return await service.GetByIdAsync(id, tenantId);
    }

    public async Task<IEnumerable<ItemCategoryDto>> GetItemCategories(
        [Service] IItemCategoryService service,
        Guid tenantId)
    {
        return await service.GetAllAsync(tenantId);
    }

    // Item Group Queries (SCM tenant-scoped service — matches REST)
    public async Task<ItemGroupDto?> GetItemGroup(
        [Service] ScmItemGroup service,
        Guid id,
        Guid tenantId)
        => await service.GetByIdAsync(id, tenantId);

    public async Task<IEnumerable<ItemGroupDto>> GetItemGroups(
        [Service] ScmItemGroup service,
        Guid tenantId,
        bool? isActive = null)
        => await service.GetAllAsync(tenantId, isActive);

    // Unit of Measure Queries (SCM tenant-scoped service — matches REST)
    public async Task<UnitOfMeasureDto?> GetUnitOfMeasure(
        [Service] ScmUnitOfMeasure service,
        Guid id,
        Guid tenantId)
        => await service.GetByIdAsync(id, tenantId);

    public async Task<IEnumerable<UnitOfMeasureDto>> GetUnitsOfMeasure(
        [Service] ScmUnitOfMeasure service,
        Guid tenantId,
        bool? isActive = null)
        => await service.GetAllAsync(tenantId, isActive);

    // Warehouse Queries (SCM tenant-scoped — matches REST tenant path)
    public async Task<WarehouseDto?> GetWarehouse(
        [Service] ScmWarehouse service,
        Guid warehouseId,
        Guid tenantId)
        => await service.GetByIdAsync(warehouseId, tenantId);

    public async Task<IEnumerable<WarehouseDto>> GetWarehouses(
        [Service] ScmWarehouse service,
        Guid tenantId,
        Guid companyId = default,
        bool activeOnly = false)
    {
        var all = await service.GetAllAsync(tenantId, companyId);
        return activeOnly ? all.Where(w => w.IsActive) : all;
    }

    public async Task<IEnumerable<WarehouseDto>> SearchWarehouses(
        [Service] ScmWarehouse service,
        string searchTerm,
        Guid tenantId,
        Guid companyId = default)
        => await service.SearchAsync(searchTerm, tenantId, companyId);

    // Item Variant Queries
    public async Task<ItemVariantDto?> GetItemVariant(
        [Service] ScmItemVariant service,
        Guid variantId,
        Guid tenantId)
        => await service.GetByIdAsync(variantId, tenantId);

    public async Task<IEnumerable<ItemVariantDto>> GetItemVariantsByItem(
        [Service] ScmItemVariant service,
        Guid itemId,
        Guid tenantId)
        => await service.GetByItemAsync(itemId, tenantId);

    // Item Pricing Queries
    public async Task<ItemPricingDto?> GetItemPricing(
        [Service] ScmItemPricing service,
        Guid pricingId,
        Guid tenantId)
        => await service.GetByIdAsync(pricingId, tenantId);

    public async Task<IEnumerable<ItemPricingDto>> GetItemPricingsByItem(
        [Service] ScmItemPricing service,
        Guid itemId,
        Guid tenantId,
        string? priceListName = null)
        => await service.GetByItemAsync(itemId, tenantId, priceListName);

    // Barcode Queries
    public async Task<BarcodeDto?> GetBarcode(
        [Service] ScmBarcode service,
        Guid barcodeId,
        Guid tenantId)
        => await service.GetByIdAsync(barcodeId, tenantId);

    public async Task<IEnumerable<BarcodeDto>> GetBarcodes(
        [Service] ScmBarcode service,
        Guid tenantId,
        Guid? itemId = null,
        Guid? variantId = null)
        => await service.GetByItemAsync(itemId, variantId, tenantId);

    // Serial Number Queries
    public async Task<SerialNumberDto?> GetSerialNumber(
        [Service] ScmSerialNumber service,
        Guid serialId,
        Guid tenantId)
        => await service.GetByIdAsync(serialId, tenantId);

    public async Task<IEnumerable<SerialNumberDto>> GetSerialNumbersByItem(
        [Service] ScmSerialNumber service,
        Guid itemId,
        Guid tenantId,
        string? status = null)
        => await service.GetByItemAsync(itemId, tenantId, status);

    // Batch Number Queries
    public async Task<BatchNumberDto?> GetBatchNumber(
        [Service] ScmBatchNumber service,
        Guid batchId,
        Guid tenantId)
        => await service.GetByIdAsync(batchId, tenantId);

    public async Task<IEnumerable<BatchNumberDto>> GetBatchNumbersByItem(
        [Service] ScmBatchNumber service,
        Guid itemId,
        Guid tenantId)
        => await service.GetByItemAsync(itemId, tenantId);

    // Reorder Rule Queries
    public async Task<ReorderRuleDto?> GetReorderRule(
        [Service] ScmReorderRule service,
        Guid ruleId,
        Guid tenantId)
        => await service.GetByIdAsync(ruleId, tenantId);

    public async Task<IEnumerable<ReorderRuleDto>> GetReorderRules(
        [Service] ScmReorderRule service,
        Guid tenantId,
        Guid? itemId = null,
        Guid? warehouseId = null)
        => await service.GetByItemAsync(itemId, warehouseId, tenantId);

    // Item Attribute Queries
    public async Task<ItemAttributeDto?> GetItemAttribute(
        [Service] ScmItemAttribute service,
        Guid attributeId,
        Guid tenantId)
        => await service.GetByIdAsync(attributeId, tenantId);

    public async Task<IEnumerable<ItemAttributeDto>> GetItemAttributes(
        [Service] ScmItemAttribute service,
        Guid tenantId,
        bool? isActive = null)
        => await service.GetAllAsync(tenantId, isActive);

    // Valuation Method Queries
    public async Task<InventoryValuationMethodDto?> GetInventoryValuationMethod(
        [Service] ScmValuationMethod service,
        Guid methodId,
        Guid tenantId)
        => await service.GetByIdAsync(methodId, tenantId);

    public async Task<IEnumerable<InventoryValuationMethodDto>> GetInventoryValuationMethods(
        [Service] ScmValuationMethod service,
        Guid tenantId,
        bool? isActive = null)
        => await service.GetAllAsync(tenantId, isActive);

    // Item Queries (SCM tenant-scoped — matches REST)
    public async Task<ItemDto?> GetItem(
        [Service] ScmItem service,
        Guid itemId,
        Guid tenantId)
        => await service.GetByIdAsync(itemId, tenantId);

    public async Task<IEnumerable<ItemDto>> GetItems(
        [Service] ScmItem service,
        Guid tenantId,
        Guid companyId = default,
        Guid? categoryId = null)
    {
        if (categoryId.HasValue && categoryId != Guid.Empty)
            return await service.GetByCategoryAsync(categoryId.Value, tenantId);
        return await service.GetAllAsync(tenantId, companyId);
    }

    public async Task<IEnumerable<ItemDto>> GetLowStockItems(
        [Service] ScmItem service,
        Guid tenantId,
        Guid companyId = default)
        => await service.GetLowStockItemsAsync(tenantId, companyId);
}

