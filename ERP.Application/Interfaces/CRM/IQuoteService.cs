using ERP.Shared.DTOs.CRM;

namespace ERP.Application.Interfaces.CRM;

public interface IQuoteService
{
    Task<QuoteDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<QuoteDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateQuoteDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateQuoteDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<QuoteDto>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<QuoteDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<QuoteDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<bool> SendQuoteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId);
    Task<Guid> ConvertToSalesOrderAsync(Guid id, Guid tenantId, Guid userId);
}
