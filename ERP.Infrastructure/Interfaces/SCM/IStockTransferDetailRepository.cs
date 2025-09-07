using ERP.Domain.SCM;

namespace ERP.Infrastructure.Interfaces.SCM;

public interface IStockTransferDetailRepository
{
    Task<StockTransferDetail?> GetByIdAsync(int id);
    Task<IEnumerable<StockTransferDetail>> GetAllAsync();
    Task<StockTransferDetail> AddAsync(StockTransferDetail entity);
    Task UpdateAsync(StockTransferDetail entity);
    Task DeleteAsync(int id);
}