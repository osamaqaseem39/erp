using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ISalesOrderLineRepository
{
    Task<SalesOrderLine?> GetByIdAsync(int id);
    Task<IEnumerable<SalesOrderLine>> GetAllAsync();
    Task<SalesOrderLine> AddAsync(SalesOrderLine entity);
    Task UpdateAsync(SalesOrderLine entity);
    Task DeleteAsync(int id);
}