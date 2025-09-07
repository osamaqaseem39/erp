using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IJournalRepository
{
    Task<Journal?> GetByIdAsync(Guid id);
    Task<IEnumerable<Journal>> GetAllAsync();
    Task<IEnumerable<Journal>> GetByStatusAsync(string status);
    Task<IEnumerable<Journal>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Journal?> GetByNumberAsync(string journalNumber);
    Task<Guid> CreateAsync(Journal journal);
    Task<bool> UpdateAsync(Journal journal);
    Task<bool> DeleteAsync(Guid id);
}