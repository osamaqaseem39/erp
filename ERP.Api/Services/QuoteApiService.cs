using ERP.Application.Interfaces;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public class QuoteApiService : IQuoteApiService
{
    private readonly IQuoteService _service;

    public QuoteApiService(IQuoteService service)
    {
        _service = service;
    }

    public async Task<QuoteDto?> GetByIdAsync(Guid id)
    {
        return await _service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<QuoteDto>> GetAllAsync()
    {
        return await _service.GetAllAsync();
    }

    public async Task<Guid> CreateAsync(CreateQuoteDto dto)
    {
        return await _service.CreateAsync(dto);
    }

    public async Task<bool> UpdateAsync(UpdateQuoteDto dto)
    {
        return await _service.UpdateAsync(dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _service.DeleteAsync(id);
    }

    public async Task<IEnumerable<QuoteDto>> GetByCustomerAsync(Guid customerId)
    {
        return await _service.GetByCustomerAsync(customerId);
    }

    public async Task<IEnumerable<QuoteDto>> GetByStatusAsync(string status)
    {
        return await _service.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<QuoteDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _service.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> SendQuoteAsync(Guid id)
    {
        return await _service.SendQuoteAsync(id);
    }

    public async Task<bool> ConvertToSalesOrderAsync(Guid id)
    {
        return await _service.ConvertToSalesOrderAsync(id);
    }
}