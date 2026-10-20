using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface ISalesOrderService
{
    Task<SalesOrderDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<SalesOrderDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateSalesOrderDto dto);
    Task<bool> UpdateAsync(UpdateSalesOrderDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<SalesOrderDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<SalesOrderDto>> GetByStatusAsync(string status);
    Task<IEnumerable<SalesOrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> ShipAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> InvoiceAsync(Guid id, Guid tenantId, Guid userId);
}
