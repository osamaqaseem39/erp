using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface IQuoteService
{
    Task<QuoteDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<QuoteDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateQuoteDto dto);
    Task<bool> UpdateAsync(UpdateQuoteDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<QuoteDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<QuoteDto>> GetByStatusAsync(string status);
    Task<IEnumerable<QuoteDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> SendQuoteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId);
    Task<Guid> ConvertToSalesOrderAsync(Guid id, Guid tenantId, Guid userId);
}
