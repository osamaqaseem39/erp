using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface ISalesOrderLineRepository
{
    Task<IEnumerable<SalesOrderLine>> GetBySalesOrderIdAsync(Guid salesOrderId);
    Task<Guid> CreateAsync(SalesOrderLine line);
    Task<bool> DeleteBySalesOrderIdAsync(Guid salesOrderId);
}
