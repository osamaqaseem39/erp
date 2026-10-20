using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IStockTransferService
{
    Task<StockTransferDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockTransferDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateStockTransferDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateStockTransferDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockTransferDto>> GetByFromWarehouseAsync(Guid warehouseId, Guid tenantId);
    Task<IEnumerable<StockTransferDto>> GetByToWarehouseAsync(Guid warehouseId, Guid tenantId);
    Task<IEnumerable<StockTransferDto>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId);
    Task<IEnumerable<StockTransferDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
}
