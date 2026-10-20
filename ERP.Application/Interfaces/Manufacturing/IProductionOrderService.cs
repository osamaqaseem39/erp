using ERP.Shared.DTOs.Manufacturing;

namespace ERP.Application.Interfaces.Manufacturing;

public interface IProductionOrderService
{
    Task<ProductionOrderDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProductionOrderDto>> GetAllAsync(Guid tenantId, string? status = null, Guid? workCenterId = null);
    Task<IEnumerable<ProductionOrderDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<ProductionOrderDto>> GetByWorkCenterAsync(Guid workCenterId, Guid tenantId);
    Task<ProductionOrderDto> CreateAsync(CreateProductionOrderDto dto, Guid tenantId, Guid userId);
    Task<ProductionOrderDto> UpdateAsync(UpdateProductionOrderDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<ProductionOrderDto> StartAsync(Guid id, Guid tenantId, Guid userId);
    Task<ProductionOrderDto> CompleteAsync(Guid id, CompleteProductionOrderDto dto, Guid tenantId, Guid userId);
    Task<ProductionOrderDto> CancelAsync(Guid id, string reason, Guid tenantId, Guid userId);
    Task<IReadOnlyList<ProductionOrderOperationDto>> ListOperationsAsync(Guid productionOrderId, Guid tenantId);
    Task<ProductionOrderOperationDto> CompleteOperationAsync(
        Guid productionOrderId, Guid operationId, CompleteProductionOrderOperationDto dto, Guid tenantId, Guid userId);
    Task<WorkCenterCapacityWarnDto> GetCapacityWarnAsync(Guid workCenterId, Guid tenantId, DateTime? weekStartUtc = null);
}