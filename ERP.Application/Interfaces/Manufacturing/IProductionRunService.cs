using ERP.Shared.DTOs.Manufacturing;

namespace ERP.Application.Interfaces.Manufacturing;

public interface IProductionRunService
{
    Task<ProductionRunDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProductionRunDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ProductionRunDto>> GetByOrderAsync(Guid productionOrderId, Guid tenantId);
    Task<ProductionRunDto> CreateAsync(CreateProductionRunDto dto, Guid tenantId, Guid userId);
    Task<ProductionRunDto> CompleteAsync(Guid id, decimal quantityProduced, decimal quantityRejected, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
}
