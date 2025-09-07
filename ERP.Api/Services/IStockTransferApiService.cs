using ERP.Shared.DTOs.Inventory;

namespace ERP.Api.Services;

public interface IStockTransferApiService : IApiService<StockTransferDto, CreateStockTransferDto, UpdateStockTransferDto>
{
    Task<IEnumerable<StockTransferDto>> GetByFromWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<StockTransferDto>> GetByToWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<StockTransferDto>> GetByStatusAsync(string status);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> CompleteAsync(Guid id);
}