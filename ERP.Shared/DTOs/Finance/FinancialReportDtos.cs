namespace ERP.Shared.DTOs.Finance;

public class TrialBalanceLineDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public int Level { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
}

public class TrialBalanceReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public List<TrialBalanceLineDto> Lines { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

public class RunningTrialBalanceLineDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public int Level { get; set; }
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class RunningTrialBalanceReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public List<RunningTrialBalanceLineDto> Lines { get; set; } = new();
}

public class LedgerLineDto
{
    public Guid EntryId { get; set; }
    public Guid JournalId { get; set; }
    public string? JournalNumber { get; set; }
    public string? JournalType { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Narration { get; set; }
    public decimal RunningBalance { get; set; }
}

public class LedgerReportDto
{
    public Guid AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public List<LedgerLineDto> Entries { get; set; } = new();
    public decimal ClosingBalance { get; set; }
}

public class ProfitAndLossLineDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class ProfitAndLossReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public List<ProfitAndLossLineDto> Revenue { get; set; } = new();
    public List<ProfitAndLossLineDto> Expenses { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome { get; set; }
}

public class BalanceSheetSectionDto
{
    public string Section { get; set; } = string.Empty;
    public List<TrialBalanceLineDto> Lines { get; set; } = new();
    public decimal Total { get; set; }
}

public class BalanceSheetReportDto
{
    public DateTime AsOfDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public BalanceSheetSectionDto Assets { get; set; } = new() { Section = "Assets" };
    public BalanceSheetSectionDto Liabilities { get; set; } = new() { Section = "Liabilities" };
    public BalanceSheetSectionDto Equity { get; set; } = new() { Section = "Equity" };
    public decimal NetIncomePlug { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilitiesAndEquity { get; set; }
}

public class VoucherReportLineDto
{
    public Guid JournalId { get; set; }
    public string JournalNumber { get; set; } = string.Empty;
    public string JournalType { get; set; } = string.Empty;
    public DateTime JournalDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

public class VoucherReportDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? JournalType { get; set; }
    public string? Status { get; set; }
    public List<VoucherReportLineDto> Lines { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public int Count { get; set; }
}

public class ChartOfAccountReportNodeDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool IsPostingAllowed { get; set; }
    public string? Remarks { get; set; }
    public List<ChartOfAccountReportNodeDto> Children { get; set; } = new();
}

public class ChartOfAccountReportDto
{
    public bool IncludeDetailAccounts { get; set; }
    public List<ChartOfAccountReportNodeDto> Roots { get; set; } = new();
}

// --- R2: cash / partner summaries + aging ---

public class DailyFinancialTypeTotalDto
{
    public string JournalType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

public class DailyFinancialReportDto
{
    public DateTime Date { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public List<DailyFinancialTypeTotalDto> ByType { get; set; } = new();
    public decimal CashPayments { get; set; }
    public decimal CashReceipts { get; set; }
    public decimal BankPayments { get; set; }
    public decimal BankReceipts { get; set; }
    public int VoucherCount { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

public class CashBankVoucherSummaryLineDto
{
    public string JournalType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

public class CashBankVoucherSummaryReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<CashBankVoucherSummaryLineDto> Lines { get; set; } = new();
}

public class CashBankLedgerSummaryLineDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class CashBankLedgerSummaryReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<CashBankLedgerSummaryLineDto> Lines { get; set; } = new();
}

public class VoucherTypeMonthlyLineDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Period { get; set; } = string.Empty; // yyyy-MM
    public string JournalType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

public class VoucherTypeMonthlyReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<VoucherTypeMonthlyLineDto> Lines { get; set; } = new();
}

public class PartnerBalanceLineDto
{
    public Guid PartnerId { get; set; }
    public string PartnerKind { get; set; } = string.Empty; // Customer / Supplier
    public int OpenDocuments { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal Balance { get; set; }
}

public class PartnerBalanceSummaryReportDto
{
    public DateTime AsOfDate { get; set; }
    public List<PartnerBalanceLineDto> Customers { get; set; } = new();
    public List<PartnerBalanceLineDto> Suppliers { get; set; } = new();
    public decimal TotalReceivables { get; set; }
    public decimal TotalPayables { get; set; }
}

public class AgingBucketDto
{
    public string Label { get; set; } = string.Empty;
    public int? FromDays { get; set; }
    public int? ToDays { get; set; }
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class AgingDocumentLineDto
{
    public Guid DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public Guid PartnerId { get; set; }
    public DateTime DocumentDate { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public string Bucket { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class AgingReportDto
{
    public DateTime AsOfDate { get; set; }
    public string Kind { get; set; } = string.Empty; // Receivables / Payables
    public int[] BucketsUsed { get; set; } = [];
    public List<AgingBucketDto> Buckets { get; set; } = new();
    public List<AgingDocumentLineDto> Lines { get; set; } = new();
    public decimal TotalBalance { get; set; }
}

public class CashFlowLineDto
{
    public string JournalType { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty; // Inflow / Outflow
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class CashFlowReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal OpeningCash { get; set; }
    public List<CashFlowLineDto> Inflows { get; set; } = new();
    public List<CashFlowLineDto> Outflows { get; set; } = new();
    public decimal TotalInflows { get; set; }
    public decimal TotalOutflows { get; set; }
    public decimal NetChange { get; set; }
    public decimal ClosingCash { get; set; }
}

public class ChequeRegisterLineDto
{
    public Guid JournalId { get; set; }
    public string JournalNumber { get; set; } = string.Empty;
    public string JournalType { get; set; } = string.Empty;
    public DateTime JournalDate { get; set; }
    public DateTime? ChequeDate { get; set; }
    public string ChequeNumber { get; set; } = string.Empty;
    public string ChequeStatus { get; set; } = string.Empty;
    public DateTime? ChequeStatusAt { get; set; }
    public string? ChequeStatusNotes { get; set; }
    public string VoucherStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Direction { get; set; } = string.Empty; // Outflow / Inflow
    public Guid? BankAccountId { get; set; }
    public string? BankAccountName { get; set; }
    public string? PaidBy { get; set; }
    public string? ReceivedBy { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Description { get; set; }
}

public class ChequeRegisterReportDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? ChequeStatus { get; set; }
    public string? JournalType { get; set; }
    public List<ChequeRegisterLineDto> Lines { get; set; } = new();
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal ClearedAmount { get; set; }
    public decimal BouncedAmount { get; set; }
}

public class SetChequeStatusDto
{
    public string Status { get; set; } = string.Empty; // Cleared | Bounced | Cancelled | Outstanding
    public string? Notes { get; set; }
}

public class BudgetVsActualLineDto
{
    public Guid BudgetId { get; set; }
    public string BudgetName { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? AccountType { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercent { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BudgetVsActualReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Guid? BudgetId { get; set; }
    public List<BudgetVsActualLineDto> Lines { get; set; } = new();
    public decimal TotalBudget { get; set; }
    public decimal TotalActual { get; set; }
    public decimal TotalVariance { get; set; }
}
