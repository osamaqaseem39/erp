using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces;

public interface IWarehouseService
{
    Task<WarehouseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<WarehouseDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateWarehouseDto dto);
    Task<bool> UpdateAsync(UpdateWarehouseDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<WarehouseDto>> GetByLocationAsync(string location);
    Task<IEnumerable<StockBalanceDto>> GetStockBalancesAsync(Guid warehouseId);
    Task<bool> TransferStockAsync(StockTransferDto dto);
}