using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface IQuoteLineRepository
{
    Task<IEnumerable<QuoteLine>> GetByQuoteIdAsync(Guid quoteId);
    Task<Guid> CreateAsync(QuoteLine line);
    Task<bool> DeleteByQuoteIdAsync(Guid quoteId);
}
