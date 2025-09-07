using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface IPOSTransactionRepository
{
    Task<POSTransaction?> GetByIdAsync(int id);
    Task<IEnumerable<POSTransaction>> GetAllAsync();
    Task<POSTransaction> AddAsync(POSTransaction entity);
    Task UpdateAsync(POSTransaction entity);
    Task DeleteAsync(int id);
}