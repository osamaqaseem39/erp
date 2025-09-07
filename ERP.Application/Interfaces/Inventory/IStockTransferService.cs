using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IStockTransferService
{
    Task<StockTransferDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<StockTransferDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateStockTransferDto dto);
    Task<bool> UpdateAsync(UpdateStockTransferDto dto);
    Task<Guid> CreateAsync(StockTransferDto dto);
    Task<bool> UpdateAsync(StockTransferDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<StockTransferDto>> GetByFromWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<StockTransferDto>> GetByToWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<StockTransferDto>> GetByWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<StockTransferDto>> GetByStatusAsync(string status);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> CompleteAsync(Guid id);
}