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
using AppStockService = ERP.Application.Interfaces.IStockService;

namespace ERP.Api.GraphQL.Inventory;

[ExtendObjectType("Mutation")]
public class InventoryMutations
{
    // Stock balance mutations (tenant-scoped Application.IStockService)
    public async Task<StockBalanceDto> AddStockBalance(
        [Service] AppStockService service,
        StockBalanceDto input,
        Guid tenantId)
    {
        var id = await service.CreateAsync(input, tenantId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create stock balance");
    }

    public async Task<StockBalanceDto> UpdateStockBalance(
        [Service] AppStockService service,
        StockBalanceDto input,
        Guid tenantId)
    {
        var success = await service.UpdateAsync(input, tenantId);
        if (!success)
            throw new Exception("Failed to update stock balance");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Stock balance not found");
    }

    public async Task<bool> DeleteStockBalance(
        [Service] AppStockService service,
        Guid id,
        Guid tenantId)
        => await service.DeleteAsync(id, tenantId);

    public async Task<bool> AdjustStock(
        [Service] AppStockService service,
        StockAdjustmentDto input,
        Guid tenantId)
        => await service.AdjustStockAsync(input, tenantId);

    public async Task<bool> ReceiveStock(
        [Service] AppStockService service,
        Guid tenantId,
        Guid warehouseId,
        IEnumerable<StockMovementLineDto> lines,
        string? userId = null)
    {
        var list = (lines ?? []).Where(l => l.ItemId != Guid.Empty && l.Quantity > 0).ToList();
        if (list.Count == 0) return false;
        if (list.Any(l => l.UnitCost.HasValue))
            return await service.ReceiveStockAsync(
                tenantId, warehouseId,
                list.Select(l => (l.ItemId, l.Quantity, l.UnitCost ?? 0m)),
                userId);
        return await service.ReceiveStockAsync(
            tenantId, warehouseId,
            list.Select(l => (l.ItemId, l.Quantity)),
            userId);
    }

    public async Task<bool> IssueStock(
        [Service] AppStockService service,
        Guid tenantId,
        Guid warehouseId,
        IEnumerable<StockMovementLineDto> lines,
        string? userId = null)
    {
        var list = (lines ?? []).Where(l => l.ItemId != Guid.Empty && l.Quantity > 0).ToList();
        if (list.Count == 0) return false;
        return await service.IssueStockAsync(
            tenantId, warehouseId,
            list.Select(l => (l.ItemId, l.Quantity)),
            userId);
    }

    // Stock Transfer Mutations
    public async Task<StockTransferDto> AddStockTransfer(
        [Service] IStockTransferService service,
        CreateStockTransferDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create stock transfer");
    }

    public async Task<StockTransferDto> UpdateStockTransfer(
        [Service] IStockTransferService service,
        UpdateStockTransferDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update stock transfer");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Stock transfer not found");
    }

    public async Task<bool> DeleteStockTransfer(
        [Service] IStockTransferService service,
        Guid id,
        Guid tenantId)
    {
        return await service.DeleteAsync(id, tenantId);
    }

    public async Task<bool> ApproveStockTransfer(
        [Service] IStockTransferService service,
        Guid id,
        Guid tenantId,
        Guid userId)
    {
        return await service.ApproveAsync(id, tenantId, userId);
    }

    public async Task<bool> CompleteStockTransfer(
        [Service] IStockTransferService service,
        Guid id,
        Guid tenantId,
        Guid userId)
    {
        return await service.CompleteAsync(id, tenantId, userId);
    }

    public async Task<bool> CancelStockTransfer(
        [Service] IStockTransferService service,
        Guid id,
        Guid tenantId,
        Guid userId)
    {
        return await service.CancelAsync(id, tenantId, userId);
    }

    // Stock Adjustment Mutations
    public async Task<StockAdjustmentDto> AddStockAdjustment(
        [Service] IStockAdjustmentService service,
        CreateStockAdjustmentDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create stock adjustment");
    }

    public async Task<StockAdjustmentDto> UpdateStockAdjustment(
        [Service] IStockAdjustmentService service,
        UpdateStockAdjustmentDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update stock adjustment");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Stock adjustment not found");
    }

    public async Task<bool> DeleteStockAdjustment(
        [Service] IStockAdjustmentService service,
        Guid id,
        Guid tenantId)
    {
        return await service.DeleteAsync(id, tenantId);
    }

    public async Task<bool> PostStockAdjustment(
        [Service] IStockAdjustmentService service,
        Guid id,
        Guid tenantId,
        Guid userId)
    {
        return await service.PostAsync(id, tenantId, userId);
    }

    // Item Category Mutations
    public async Task<ItemCategoryDto> AddItemCategory(
        [Service] IItemCategoryService service,
        CreateItemCategoryDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create item category");
    }

    public async Task<ItemCategoryDto> UpdateItemCategory(
        [Service] IItemCategoryService service,
        UpdateItemCategoryDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update item category");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Item category not found");
    }

    public async Task<bool> DeleteItemCategory(
        [Service] IItemCategoryService service,
        Guid id,
        Guid tenantId)
    {
        return await service.DeleteAsync(id, tenantId);
    }

    // Item Group Mutations (SCM tenant-scoped — matches REST)
    public async Task<ItemGroupDto> AddItemGroup(
        [Service] ScmItemGroup service,
        CreateItemGroupDto input,
        Guid tenantId,
        Guid userId)
    {
        var dto = new ItemGroupDto
        {
            GroupCode = input.GroupCode,
            GroupName = input.GroupName,
            Description = input.Description,
            ParentGroupId = input.ParentGroupId,
            Notes = input.Notes,
            IsActive = true
        };
        return await service.CreateAsync(dto, tenantId, userId.ToString());
    }

    public async Task<ItemGroupDto> UpdateItemGroup(
        [Service] ScmItemGroup service,
        UpdateItemGroupDto input,
        Guid tenantId,
        Guid userId)
    {
        var dto = new ItemGroupDto
        {
            Id = input.Id,
            GroupCode = input.GroupCode,
            GroupName = input.GroupName,
            Description = input.Description,
            ParentGroupId = input.ParentGroupId,
            Notes = input.Notes
        };
        return await service.UpdateAsync(dto, tenantId, userId.ToString());
    }

    public async Task<bool> DeleteItemGroup(
        [Service] ScmItemGroup service,
        Guid id,
        Guid tenantId)
        => await service.DeleteAsync(id, tenantId);

    // Unit of Measure Mutations (SCM tenant-scoped — matches REST)
    public async Task<UnitOfMeasureDto> AddUnitOfMeasure(
        [Service] ScmUnitOfMeasure service,
        CreateUnitOfMeasureDto input,
        Guid tenantId,
        Guid userId)
    {
        var dto = new UnitOfMeasureDto
        {
            Code = input.Code,
            Name = input.Name,
            Description = input.Description,
            Symbol = input.Symbol,
            ConversionFactor = input.ConversionFactor,
            BaseUnitId = input.BaseUnitId,
            IsBaseUnit = input.IsBaseUnit,
            IsActive = input.IsActive
        };
        return await service.CreateAsync(dto, tenantId, userId.ToString());
    }

    public async Task<UnitOfMeasureDto> UpdateUnitOfMeasure(
        [Service] ScmUnitOfMeasure service,
        UpdateUnitOfMeasureDto input,
        Guid tenantId,
        Guid userId)
    {
        var dto = new UnitOfMeasureDto
        {
            Id = input.Id,
            Code = input.Code,
            Name = input.Name,
            Description = input.Description,
            Symbol = input.Symbol,
            ConversionFactor = input.ConversionFactor,
            BaseUnitId = input.BaseUnitId,
            IsBaseUnit = input.IsBaseUnit,
            IsActive = input.IsActive
        };
        return await service.UpdateAsync(dto, tenantId, userId.ToString());
    }

    public async Task<bool> DeleteUnitOfMeasure(
        [Service] ScmUnitOfMeasure service,
        Guid id,
        Guid tenantId)
        => await service.DeleteAsync(id, tenantId);

    // Warehouse Mutations (SCM tenant-scoped — matches REST tenant path)
    public async Task<WarehouseDto> AddWarehouse(
        [Service] ScmWarehouse service,
        CreateWarehouseDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId.ToString());

    public async Task<WarehouseDto> UpdateWarehouse(
        [Service] ScmWarehouse service,
        UpdateWarehouseDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId.ToString());

    public async Task<bool> DeleteWarehouse(
        [Service] ScmWarehouse service,
        Guid warehouseId,
        Guid tenantId)
        => await service.DeleteAsync(warehouseId, tenantId);

    public async Task<bool> ActivateWarehouse(
        [Service] ScmWarehouse service,
        Guid warehouseId,
        Guid tenantId,
        Guid userId)
        => await service.ActivateAsync(warehouseId, tenantId, userId.ToString());

    public async Task<bool> DeactivateWarehouse(
        [Service] ScmWarehouse service,
        Guid warehouseId,
        Guid tenantId,
        Guid userId)
        => await service.DeactivateAsync(warehouseId, tenantId, userId.ToString());

    // Item Variant Mutations
    public async Task<ItemVariantDto> AddItemVariant(
        [Service] ScmItemVariant service,
        ItemVariantDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId.ToString());

    public async Task<ItemVariantDto> UpdateItemVariant(
        [Service] ScmItemVariant service,
        ItemVariantDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId.ToString());

    public async Task<bool> DeleteItemVariant(
        [Service] ScmItemVariant service,
        Guid variantId,
        Guid tenantId)
        => await service.DeleteAsync(variantId, tenantId);

    // Item Pricing Mutations
    public async Task<ItemPricingDto> AddItemPricing(
        [Service] ScmItemPricing service,
        ItemPricingDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId.ToString());

    public async Task<ItemPricingDto> UpdateItemPricing(
        [Service] ScmItemPricing service,
        ItemPricingDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId.ToString());

    public async Task<bool> DeleteItemPricing(
        [Service] ScmItemPricing service,
        Guid pricingId,
        Guid tenantId)
        => await service.DeleteAsync(pricingId, tenantId);

    // Barcode Mutations
    public async Task<BarcodeDto> AddBarcode(
        [Service] ScmBarcode service,
        BarcodeDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId.ToString());

    public async Task<BarcodeDto> UpdateBarcode(
        [Service] ScmBarcode service,
        BarcodeDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId.ToString());

    public async Task<bool> DeleteBarcode(
        [Service] ScmBarcode service,
        Guid barcodeId,
        Guid tenantId)
        => await service.DeleteAsync(barcodeId, tenantId);

    // Serial Number Mutations
    public async Task<SerialNumberDto> AddSerialNumber(
        [Service] ScmSerialNumber service,
        SerialNumberDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId.ToString());

    public async Task<SerialNumberDto> UpdateSerialNumber(
        [Service] ScmSerialNumber service,
        SerialNumberDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId.ToString());

    public async Task<bool> DeleteSerialNumber(
        [Service] ScmSerialNumber service,
        Guid serialId,
        Guid tenantId)
        => await service.DeleteAsync(serialId, tenantId);

    // Batch Number Mutations
    public async Task<BatchNumberDto> AddBatchNumber(
        [Service] ScmBatchNumber service,
        BatchNumberDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId.ToString());

    public async Task<BatchNumberDto> UpdateBatchNumber(
        [Service] ScmBatchNumber service,
        BatchNumberDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId.ToString());

    public async Task<bool> DeleteBatchNumber(
        [Service] ScmBatchNumber service,
        Guid batchId,
        Guid tenantId)
        => await service.DeleteAsync(batchId, tenantId);

    // Reorder Rule Mutations
    public async Task<ReorderRuleDto> AddReorderRule(
        [Service] ScmReorderRule service,
        ReorderRuleDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId.ToString());

    public async Task<ReorderRuleDto> UpdateReorderRule(
        [Service] ScmReorderRule service,
        ReorderRuleDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId.ToString());

    public async Task<bool> DeleteReorderRule(
        [Service] ScmReorderRule service,
        Guid ruleId,
        Guid tenantId)
        => await service.DeleteAsync(ruleId, tenantId);

    // Item Attribute Mutations
    public async Task<ItemAttributeDto> AddItemAttribute(
        [Service] ScmItemAttribute service,
        ItemAttributeDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId.ToString());

    public async Task<ItemAttributeDto> UpdateItemAttribute(
        [Service] ScmItemAttribute service,
        ItemAttributeDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId.ToString());

    public async Task<bool> DeleteItemAttribute(
        [Service] ScmItemAttribute service,
        Guid attributeId,
        Guid tenantId)
        => await service.DeleteAsync(attributeId, tenantId);

    // Valuation Method Mutations
    public async Task<InventoryValuationMethodDto> AddInventoryValuationMethod(
        [Service] ScmValuationMethod service,
        InventoryValuationMethodDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId.ToString());

    public async Task<InventoryValuationMethodDto> UpdateInventoryValuationMethod(
        [Service] ScmValuationMethod service,
        InventoryValuationMethodDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId.ToString());

    public async Task<bool> DeleteInventoryValuationMethod(
        [Service] ScmValuationMethod service,
        Guid methodId,
        Guid tenantId)
        => await service.DeleteAsync(methodId, tenantId);

    // Item Mutations (SCM tenant-scoped — matches REST)
    public async Task<ItemDto> AddItem(
        [Service] ScmItem service,
        CreateItemDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId.ToString());

    public async Task<ItemDto> UpdateItem(
        [Service] ScmItem service,
        UpdateItemDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId.ToString());

    public async Task<bool> DeleteItem(
        [Service] ScmItem service,
        Guid itemId,
        Guid tenantId)
        => await service.DeleteAsync(itemId, tenantId);
}

