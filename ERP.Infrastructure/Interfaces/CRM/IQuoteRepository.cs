using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface IQuoteRepository
{
    Task<Quote?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Quote>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<Quote>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<Quote>> GetByStatusAsync(string status, Guid tenantId);
    Task<Quote?> GetByNumberAsync(string quoteNumber, Guid tenantId);
    Task<IEnumerable<Quote>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<Guid> CreateAsync(Quote quote);
    Task<bool> UpdateAsync(Quote quote);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
