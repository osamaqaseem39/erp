using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesOrder>> GetAllAsync(Guid tenantId);
    Task<SalesOrder> AddAsync(SalesOrder entity);
    Task UpdateAsync(SalesOrder entity);
    Task DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesOrder>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<SalesOrder>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<SalesOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
}