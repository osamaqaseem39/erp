using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IAccountingService
{
    // Chart of Accounts
    Task<Guid> AddChartOfAccountAsync(CreateChartOfAccountDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateChartOfAccountAsync(UpdateChartOfAccountDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteChartOfAccountAsync(Guid accountId, Guid tenantId);
    Task<ChartOfAccountDto?> GetChartOfAccountByIdAsync(Guid accountId, Guid tenantId);
    Task<IEnumerable<ChartOfAccountDto>> GetAllChartOfAccountsAsync(Guid tenantId, string? accountType = null, bool? isActive = null);
    
    // Journals
    Task<Guid> AddJournalAsync(JournalCreateDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateJournalAsync(JournalUpdateDto dto, Guid tenantId, Guid userId);
    Task<Guid> PostJournalAsync(Guid journalId, Guid tenantId, Guid postedBy);
    Task<Guid> ReverseJournalAsync(Guid journalId, Guid tenantId, Guid reversedBy, string? reversalReason = null);
    Task<JournalDto?> GetJournalByIdAsync(Guid journalId, Guid tenantId);
    Task<(IEnumerable<JournalDto> Journals, int TotalCount)> SearchJournalsAsync(JournalQuery query);
    
    // Journal Entries
    Task<Guid> AddJournalEntryAsync(CreateJournalEntryDto dto, Guid journalId, Guid tenantId, Guid userId);
    Task<Guid> UpdateJournalEntryAsync(UpdateJournalEntryDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteJournalEntryAsync(Guid entryId, Guid tenantId);
    Task<IEnumerable<JournalEntryDto>> GetJournalEntriesByJournalIdAsync(Guid journalId, Guid tenantId);
    
    // Financial Reports
    Task<object> GetTrialBalanceAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null);
    Task<object> GetGeneralLedgerBalanceAsync(Guid accountId, Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<object> GetProfitAndLossAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null);
    Task<object> GetBalanceSheetAsync(Guid tenantId, DateTime asOfDate, string? currencyCode = null);
}