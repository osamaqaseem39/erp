using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IJournalEntryRepository
{
    Task<JournalEntry?> GetByIdAsync(Guid id);
    Task<IEnumerable<JournalEntry>> GetAllAsync();
    Task<Guid> CreateAsync(JournalEntry journalEntry);
    Task<bool> UpdateAsync(JournalEntry journalEntry);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<JournalEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<JournalEntry>> GetByReferenceAsync(string reference);
}