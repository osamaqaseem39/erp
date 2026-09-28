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
        Guid tenantId,
        Guid userId)
    {
        return await service.CompleteAsync(productionOrderId, tenantId, userId);
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
}

