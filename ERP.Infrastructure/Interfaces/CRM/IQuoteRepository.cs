using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface IQuoteRepository
{
    Task<Quote?> GetByIdAsync(Guid id);
    Task<IEnumerable<Quote>> GetAllAsync();
    Task<Guid> CreateAsync(Quote quote);
    Task<bool> UpdateAsync(Quote quote);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Quote?> GetByNumberAsync(string number);
    Task<bool> IsNumberUniqueAsync(string number, Guid? excludeId = null);
    Task<IEnumerable<Quote>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Quote>> GetByStatusAsync(string status);
    Task<IEnumerable<Quote>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}