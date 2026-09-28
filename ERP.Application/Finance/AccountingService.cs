using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using System;
using System.Linq;

namespace ERP.Application.Finance;

/// <summary>DI stub for IAccountingService. Replace with real business logic incrementally.</summary>
public class AccountingService : IAccountingService
{
    public Task<Guid> AddChartOfAccountAsync(CreateChartOfAccountDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<Guid> UpdateChartOfAccountAsync(UpdateChartOfAccountDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<bool> DeleteChartOfAccountAsync(Guid accountId, Guid tenantId)
    {
        return Task.FromResult(false);
    }

    public Task<ChartOfAccountDto?> GetChartOfAccountByIdAsync(Guid accountId, Guid tenantId)
    {
        return Task.FromResult<ChartOfAccountDto?>(null);
    }

    public Task<IEnumerable<ChartOfAccountDto>> GetAllChartOfAccountsAsync(Guid tenantId, string? accountType = null, bool? isActive = null)
    {
        return Task.FromResult(Enumerable.Empty<ChartOfAccountDto>());
    }

    public Task<Guid> AddJournalAsync(JournalCreateDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<Guid> UpdateJournalAsync(JournalUpdateDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<Guid> PostJournalAsync(Guid journalId, Guid tenantId, Guid postedBy)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<Guid> ReverseJournalAsync(Guid journalId, Guid tenantId, Guid reversedBy, string? reversalReason = null)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<JournalDto?> GetJournalByIdAsync(Guid journalId, Guid tenantId)
    {
        return Task.FromResult<JournalDto?>(null);
    }

    public Task<(IEnumerable<JournalDto> Journals, int TotalCount)> SearchJournalsAsync(JournalQuery query)
    {
        throw new NotImplementedException("AccountingService.SearchJournalsAsync is not yet implemented");
    }

    public Task<Guid> AddJournalEntryAsync(CreateJournalEntryDto dto, Guid journalId, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<Guid> UpdateJournalEntryAsync(UpdateJournalEntryDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<bool> DeleteJournalEntryAsync(Guid entryId, Guid tenantId)
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<JournalEntryDto>> GetJournalEntriesByJournalIdAsync(Guid journalId, Guid tenantId)
    {
        return Task.FromResult(Enumerable.Empty<JournalEntryDto>());
    }

    public Task<object> GetTrialBalanceAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
    {
        return Task.FromResult<object>(new { });
    }

    public Task<object> GetGeneralLedgerBalanceAsync(Guid accountId, Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        return Task.FromResult<object>(new { });
    }

    public Task<object> GetProfitAndLossAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
    {
        return Task.FromResult<object>(new { });
    }

    public Task<object> GetBalanceSheetAsync(Guid tenantId, DateTime asOfDate, string? currencyCode = null)
    {
        return Task.FromResult<object>(new { });
    }

}
