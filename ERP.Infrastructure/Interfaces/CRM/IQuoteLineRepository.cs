using ERP.Domain.CRM;

namespace ERP.Infrastructure.Interfaces.CRM;

public interface IQuoteLineRepository
{
    Task<QuoteLine?> GetByIdAsync(int id);
    Task<IEnumerable<QuoteLine>> GetAllAsync();
    Task<QuoteLine> AddAsync(QuoteLine entity);
    Task UpdateAsync(QuoteLine entity);
    Task DeleteAsync(int id);
}