using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<SalesOrder>> GetAllAsync();
    Task<IEnumerable<SalesOrder>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<SalesOrder>> GetByStatusAsync(string status);
    Task<SalesOrder?> GetByNumberAsync(string salesOrderNumber);
    Task<IEnumerable<SalesOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Guid> CreateAsync(SalesOrder salesOrder);
    Task<bool> UpdateAsync(SalesOrder salesOrder);
    Task<bool> DeleteAsync(Guid id);
}
