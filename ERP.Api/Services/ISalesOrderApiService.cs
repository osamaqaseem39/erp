using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public interface ISalesOrderApiService : IApiService<SalesOrderDto, CreateSalesOrderDto, UpdateSalesOrderDto>
{
    Task<IEnumerable<SalesOrderDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<SalesOrderDto>> GetByStatusAsync(string status);
    Task<IEnumerable<SalesOrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> ShipAsync(Guid id);
    Task<bool> InvoiceAsync(Guid id);
}