using ERP.Application.Interfaces.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;
using HotChocolate;

namespace ERP.Api.GraphQL.Manufacturing;

[ExtendObjectType("Query")]
public class ManufacturingQueries
{
    // Production Order Queries
    public async Task<ProductionOrderDto?> GetProductionOrder(
        [Service] IProductionOrderService service,
        Guid productionOrderId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(productionOrderId, tenantId);
    }

    public async Task<IEnumerable<ProductionOrderDto>> GetProductionOrders(
        [Service] IProductionOrderService service,
        Guid tenantId,
        string? status = null,
        Guid? workCenterId = null)
    {
        return await service.GetAllAsync(tenantId, status, workCenterId);
    }

    public async Task<IEnumerable<ProductionOrderDto>> GetProductionOrdersByStatus(
        [Service] IProductionOrderService service,
        string status,
        Guid tenantId)
    {
        return await service.GetByStatusAsync(status, tenantId);
    }

    public async Task<IEnumerable<ProductionOrderDto>> GetProductionOrdersByWorkCenter(
        [Service] IProductionOrderService service,
        Guid workCenterId,
        Guid tenantId)
    {
        return await service.GetByWorkCenterAsync(workCenterId, tenantId);
    }

    // Work Center Queries
    public async Task<WorkCenterDto?> GetWorkCenter(
        [Service] IWorkCenterService service,
        Guid workCenterId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(workCenterId, tenantId);
    }

    public async Task<IEnumerable<WorkCenterDto>> GetWorkCenters(
        [Service] IWorkCenterService service,
        Guid tenantId)
    {
        return await service.GetAllAsync(tenantId);
    }

    // Bill of Materials
    public async Task<BillOfMaterialDto?> GetBillOfMaterial(
        [Service] IBillOfMaterialService service,
        Guid bomId,
        Guid tenantId)
        => await service.GetByIdAsync(bomId, tenantId);

    public async Task<IEnumerable<BillOfMaterialDto>> GetBillOfMaterials(
        [Service] IBillOfMaterialService service,
        Guid tenantId,
        Guid? finishedGoodItemId = null)
        => await service.GetAllAsync(tenantId, finishedGoodItemId);

    // Production Runs
    public async Task<ProductionRunDto?> GetProductionRun(
        [Service] IProductionRunService service,
        Guid productionRunId,
        Guid tenantId)
        => await service.GetByIdAsync(productionRunId, tenantId);

    public async Task<IEnumerable<ProductionRunDto>> GetProductionRunsByOrder(
        [Service] IProductionRunService service,
        Guid productionOrderId,
        Guid tenantId)
        => await service.GetByOrderAsync(productionOrderId, tenantId);
}

