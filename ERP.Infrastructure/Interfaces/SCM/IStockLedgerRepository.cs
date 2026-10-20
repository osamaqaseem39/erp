using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IStockLedgerRepository
{
    Task<StockLedger?> GetByIdAsync(Guid id);
    Task<IEnumerable<StockLedger>> GetAllAsync();
    Task<IEnumerable<StockLedger>> GetByItemAsync(Guid itemId, Guid tenantId, Guid? warehouseId = null, DateTime? toDate = null);
    Task<StockLedger> AddAsync(StockLedger entity);
    Task UpdateAsync(StockLedger entity);
    Task DeleteAsync(Guid id);
}