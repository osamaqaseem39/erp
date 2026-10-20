using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IStockTransferRepository
{
    Task<StockTransfer?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockTransfer>> GetAllAsync(Guid tenantId);
    Task<StockTransfer> CreateAsync(StockTransfer entity);
    Task<StockTransfer> UpdateAsync(StockTransfer entity);
    Task ReplaceDetailsAsync(Guid transferId, Guid tenantId, IReadOnlyList<StockTransferDetail> lines);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<StockTransfer>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId);
    Task<IEnumerable<StockTransfer>> GetByStatusAsync(string status, Guid tenantId);
}