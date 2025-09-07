using ERP.Shared.DTOs.CRM;

namespace ERP.Api.Services;

public interface IQuoteApiService : IApiService<QuoteDto, CreateQuoteDto, UpdateQuoteDto>
{
    Task<IEnumerable<QuoteDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<QuoteDto>> GetByStatusAsync(string status);
    Task<IEnumerable<QuoteDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> SendQuoteAsync(Guid id);
    Task<bool> ConvertToSalesOrderAsync(Guid id);
}