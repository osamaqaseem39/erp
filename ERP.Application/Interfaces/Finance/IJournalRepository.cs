using ERP.Domain.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IJournalRepository
{
    // Journals
    Task<Journal?> GetByIdAsync(Guid journalId, Guid tenantId);
    Task<IEnumerable<Journal>> GetAllAsync(Guid tenantId, string? journalType = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50);
    Task<int> GetCountAsync(Guid tenantId, string? journalType = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<Journal> AddAsync(Journal journal);
    Task<Journal> UpdateAsync(Journal journal);
    Task<bool> DeleteAsync(Guid journalId, Guid tenantId);
    Task<bool> IsLockedAsync(Guid journalId, Guid tenantId);
    Task<bool> PostJournalAsync(Guid journalId, Guid tenantId, Guid postedBy, bool allowPostWithoutApprove = true);
    Task<bool> UnpostJournalAsync(Guid journalId, Guid tenantId, Guid unpostedBy);
    Task<bool> ReverseJournalAsync(Guid journalId, Guid tenantId, Guid reversedBy, string? reversalReason = null);
    
    // Journal Entries
    Task<IEnumerable<JournalEntry>> GetEntriesByJournalIdAsync(Guid journalId, Guid tenantId);
    Task<JournalEntry?> GetEntryByIdAsync(Guid entryId, Guid tenantId);
    Task<JournalEntry> AddEntryAsync(JournalEntry entry);
    Task<JournalEntry> UpdateEntryAsync(JournalEntry entry);
    Task<bool> DeleteEntryAsync(Guid entryId, Guid tenantId);
    Task<decimal> GetTotalDebitAsync(Guid journalId, Guid tenantId);
    Task<decimal> GetTotalCreditAsync(Guid journalId, Guid tenantId);
    Task<bool> IsJournalBalancedAsync(Guid journalId, Guid tenantId);
    
    // Financial Queries
    Task<IEnumerable<JournalEntry>> GetEntriesByAccountAsync(Guid accountId, Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<decimal> GetAccountBalanceAsync(Guid accountId, Guid tenantId, DateTime? asOfDate = null);
    Task<IEnumerable<JournalEntry>> GetEntriesByEntityReferenceAsync(string entityReferenceTable, Guid entityReferenceId, Guid tenantId);
}