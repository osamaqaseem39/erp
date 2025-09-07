using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public class SalesOrderApiService : ISalesOrderApiService
{
    private readonly ISalesOrderService _service;

    public SalesOrderApiService(ISalesOrderService service)
    {
        _service = service;
    }

    public async Task<SalesOrderDto?> GetByIdAsync(Guid id)
    {
        return await _service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetAllAsync()
    {
        return await _service.GetAllAsync();
    }

    public async Task<Guid> CreateAsync(CreateSalesOrderDto dto)
    {
        return await _service.CreateAsync(dto);
    }

    public async Task<bool> UpdateAsync(UpdateSalesOrderDto dto)
    {
        return await _service.UpdateAsync(dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _service.DeleteAsync(id);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByCustomerAsync(Guid customerId)
    {
        return await _service.GetByCustomerAsync(customerId);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByStatusAsync(string status)
    {
        return await _service.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _service.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> ApproveAsync(Guid id)
    {
        return await _service.ApproveAsync(id);
    }

    public async Task<bool> ShipAsync(Guid id)
    {
        return await _service.ShipAsync(id);
    }

    public async Task<bool> InvoiceAsync(Guid id)
    {
        return await _service.InvoiceAsync(id);
    }
}