using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesOrder>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<SalesOrder>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<SalesOrder>> GetByStatusAsync(string status, Guid tenantId);
    Task<SalesOrder?> GetByNumberAsync(string salesOrderNumber, Guid tenantId);
    Task<IEnumerable<SalesOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<Guid> CreateAsync(SalesOrder salesOrder);
    Task<bool> UpdateAsync(SalesOrder salesOrder);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
