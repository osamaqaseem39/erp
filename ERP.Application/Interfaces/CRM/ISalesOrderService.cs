using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface ISalesOrderService
{
    Task<SalesOrderDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesOrderDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateSalesOrderDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateSalesOrderDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesOrderDto>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<SalesOrderDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<SalesOrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> ShipAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> InvoiceAsync(Guid id, Guid tenantId, Guid userId);
}
