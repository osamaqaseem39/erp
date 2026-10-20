using ERP.Application.Interfaces.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using HotChocolate;

namespace ERP.Api.GraphQL.Manufacturing;

[ExtendObjectType("Mutation")]
public class ManufacturingMutations
{
    // Production Order Mutations
    public async Task<ProductionOrderDto> AddProductionOrder(
        [Service] IProductionOrderService service,
        CreateProductionOrderDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CreateAsync(input, tenantId, userId);
    }

    public async Task<ProductionOrderDto> UpdateProductionOrder(
        [Service] IProductionOrderService service,
        UpdateProductionOrderDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.UpdateAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteProductionOrder(
        [Service] IProductionOrderService service,
        Guid productionOrderId,
        Guid tenantId)
    {
        return await service.DeleteAsync(productionOrderId, tenantId);
    }

    public async Task<ProductionOrderDto> StartProductionOrder(
        [Service] IProductionOrderService service,
        Guid productionOrderId,
        Guid tenantId,
        Guid userId)
    {
        return await service.StartAsync(productionOrderId, tenantId, userId);
    }

    public async Task<ProductionOrderDto> CompleteProductionOrder(
        [Service] IProductionOrderService service,
        Guid productionOrderId,
        CompleteProductionOrderDto input,
        Guid tenantId,
        Guid userId)
    {
        input.ProductionOrderId = productionOrderId;
        return await service.CompleteAsync(productionOrderId, input, tenantId, userId);
    }

    public async Task<ProductionOrderDto> CancelProductionOrder(
        [Service] IProductionOrderService service,
        Guid productionOrderId,
        string reason,
        Guid tenantId,
        Guid userId)
    {
        return await service.CancelAsync(productionOrderId, reason, tenantId, userId);
    }

    // Work Center Mutations
    public async Task<WorkCenterDto> AddWorkCenter(
        [Service] IWorkCenterService service,
        CreateWorkCenterDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CreateAsync(input, tenantId, userId);
    }

    public async Task<WorkCenterDto> UpdateWorkCenter(
        [Service] IWorkCenterService service,
        UpdateWorkCenterDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.UpdateAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteWorkCenter(
        [Service] IWorkCenterService service,
        Guid workCenterId,
        Guid tenantId)
    {
        return await service.DeleteAsync(workCenterId, tenantId);
    }

    public async Task<ProductionRunDto> AddProductionRun(
        [Service] IProductionRunService service,
        CreateProductionRunDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CreateAsync(input, tenantId, userId);
    }

    public async Task<ProductionRunDto> CompleteProductionRun(
        [Service] IProductionRunService service,
        Guid productionRunId,
        decimal quantityProduced,
        decimal quantityRejected,
        Guid tenantId,
        Guid userId)
    {
        return await service.CompleteAsync(productionRunId, quantityProduced, quantityRejected, tenantId, userId);
    }

    public async Task<bool> CancelProductionRun(
        [Service] IProductionRunService service,
        Guid productionRunId,
        Guid tenantId,
        Guid userId)
        => await service.CancelAsync(productionRunId, tenantId, userId);

    // Bill of Materials
    public async Task<BillOfMaterialDto> AddBillOfMaterial(
        [Service] IBillOfMaterialService service,
        CreateBillOfMaterialDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId);

    public async Task<BillOfMaterialDto> UpdateBillOfMaterial(
        [Service] IBillOfMaterialService service,
        UpdateBillOfMaterialDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeleteBillOfMaterial(
        [Service] IBillOfMaterialService service,
        Guid bomId,
        Guid tenantId)
        => await service.DeleteAsync(bomId, tenantId);

    public async Task<BillOfMaterialDto> ActivateBillOfMaterial(
        [Service] IBillOfMaterialService service,
        Guid bomId,
        Guid tenantId,
        Guid userId)
        => await service.ActivateAsync(bomId, tenantId, userId);

    public async Task<BillOfMaterialDto> ObsoleteBillOfMaterial(
        [Service] IBillOfMaterialService service,
        Guid bomId,
        Guid tenantId,
        Guid userId)
        => await service.ObsoleteAsync(bomId, tenantId, userId);
}

