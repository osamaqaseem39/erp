using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface IPOSTransactionRepository
{
    Task<POSTransaction?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<POSTransaction>> GetAllAsync(Guid tenantId);
    Task<POSTransaction> AddAsync(POSTransaction entity);
    Task UpdateAsync(POSTransaction entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<POSTransaction>> GetBySessionAsync(Guid sessionId, Guid tenantId);
    Task<IEnumerable<POSTransaction>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<POSTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
}
