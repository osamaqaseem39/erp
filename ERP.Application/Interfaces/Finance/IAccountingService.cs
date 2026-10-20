using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IAccountingService
{
    // Chart of Accounts
    Task<Guid> AddChartOfAccountAsync(CreateChartOfAccountDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateChartOfAccountAsync(UpdateChartOfAccountDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteChartOfAccountAsync(Guid accountId, Guid tenantId);
    Task<ChartOfAccountDto?> GetChartOfAccountByIdAsync(Guid accountId, Guid tenantId);
    Task<IEnumerable<ChartOfAccountDto>> GetAllChartOfAccountsAsync(
        Guid tenantId, string? accountType = null, bool? isActive = null,
        int? level = null, Guid? parentAccountId = null, bool? postingOnly = null);
    Task<int> SeedDefaultChartOfAccountsAsync(Guid tenantId, Guid userId);

    // Journals
    /// <summary>Last-used cash/bank account and cost center for typed voucher entry.</summary>
    Task<VoucherDefaultsDto> GetVoucherDefaultsAsync(Guid tenantId);
    Task<Guid> AddJournalAsync(JournalCreateDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateJournalAsync(JournalUpdateDto dto, Guid tenantId, Guid userId);
    Task<Guid> ApproveJournalAsync(Guid journalId, Guid tenantId, Guid approvedBy);
    Task<Guid> RejectJournalAsync(Guid journalId, Guid tenantId, Guid rejectedBy, string? reason = null);
    Task<Guid> PostJournalAsync(Guid journalId, Guid tenantId, Guid postedBy, bool allowPostWithoutApprove = true);
    Task<Guid> UnpostJournalAsync(Guid journalId, Guid tenantId, Guid unpostedBy);
    Task<Guid> ReverseJournalAsync(Guid journalId, Guid tenantId, Guid reversedBy, string? reversalReason = null);
    Task<JournalDto?> GetJournalByIdAsync(Guid journalId, Guid tenantId);
    Task<(IEnumerable<JournalDto> Journals, int TotalCount)> SearchJournalsAsync(JournalQuery query);

    // Journal Entries
    Task<Guid> AddJournalEntryAsync(CreateJournalEntryDto dto, Guid journalId, Guid tenantId, Guid userId);
    Task<Guid> UpdateJournalEntryAsync(UpdateJournalEntryDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteJournalEntryAsync(Guid entryId, Guid tenantId);
    Task<IEnumerable<JournalEntryDto>> GetJournalEntriesByJournalIdAsync(Guid journalId, Guid tenantId);

    // Financial Reports
    Task<TrialBalanceReportDto> GetTrialBalanceAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null);
    Task<RunningTrialBalanceReportDto> GetRunningTrialBalanceAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null);
    Task<LedgerReportDto> GetGeneralLedgerBalanceAsync(Guid accountId, Guid tenantId, DateTime fromDate, DateTime toDate, bool postingOnly = false);
    Task<ProfitAndLossReportDto> GetProfitAndLossAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null);
    Task<BalanceSheetReportDto> GetBalanceSheetAsync(Guid tenantId, DateTime asOfDate, string? currencyCode = null);
    Task<VoucherReportDto> GetVoucherReportAsync(Guid tenantId, DateTime? fromDate, DateTime? toDate, string? journalType = null, string? status = null);
    Task<ChartOfAccountReportDto> GetChartOfAccountReportAsync(Guid tenantId, bool includeDetailAccounts = true);

    // R2 summaries
    Task<DailyFinancialReportDto> GetDailyFinancialAsync(Guid tenantId, DateTime date, string? currencyCode = null);
    Task<CashBankVoucherSummaryReportDto> GetCashBankVoucherSummaryAsync(Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<CashBankLedgerSummaryReportDto> GetCashBankLedgerSummaryAsync(Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<VoucherTypeMonthlyReportDto> GetVoucherTypeMonthlySummaryAsync(Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<CashFlowReportDto> GetCashFlowAsync(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null);
    Task<BudgetVsActualReportDto> GetBudgetVsActualAsync(Guid tenantId, DateTime fromDate, DateTime toDate, Guid? budgetId = null);
    Task<ChequeRegisterReportDto> GetChequeRegisterAsync(
        Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null,
        string? chequeStatus = null, string? journalType = null, string? search = null);
    Task<bool> SetChequeStatusAsync(Guid journalId, SetChequeStatusDto dto, Guid tenantId, Guid userId);
}
