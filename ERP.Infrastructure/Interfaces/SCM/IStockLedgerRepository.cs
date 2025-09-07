using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IStockLedgerRepository
{
    Task<StockLedger?> GetByIdAsync(int id);
    Task<IEnumerable<StockLedger>> GetAllAsync();
    Task<StockLedger> AddAsync(StockLedger entity);
    Task UpdateAsync(StockLedger entity);
    Task DeleteAsync(int id);
}