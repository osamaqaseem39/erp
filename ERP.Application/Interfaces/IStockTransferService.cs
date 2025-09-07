using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces;

public interface IStockTransferService
{
    Task<StockTransferDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<StockTransferDto>> GetAllAsync();
    Task<Guid> CreateAsync(StockTransferDto dto);
    Task<bool> UpdateAsync(StockTransferDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<StockTransferDto>> GetByWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<StockTransferDto>> GetByStatusAsync(string status);
}