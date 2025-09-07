using ERP.Domain.CRM;

namespace ERP.Infrastructure.Repositories.CRM;

public interface IQuoteRepository
{
    Task<Quote?> GetByIdAsync(Guid id);
    Task<IEnumerable<Quote>> GetAllAsync();
    Task<IEnumerable<Quote>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Quote>> GetByStatusAsync(string status);
    Task<Quote?> GetByNumberAsync(string quoteNumber);
    Task<Guid> CreateAsync(Quote quote);
    Task<bool> UpdateAsync(Quote quote);
    Task<bool> DeleteAsync(Guid id);
}