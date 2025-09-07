using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface IPOSTransactionLineRepository
{
    Task<POSTransactionLine?> GetByIdAsync(int id);
    Task<IEnumerable<POSTransactionLine>> GetAllAsync();
    Task<POSTransactionLine> AddAsync(POSTransactionLine entity);
    Task UpdateAsync(POSTransactionLine entity);
    Task DeleteAsync(int id);
}