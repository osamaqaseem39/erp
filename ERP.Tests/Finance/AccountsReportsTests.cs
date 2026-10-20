using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class AccountsReportsTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AccountingService Svc(ERPDbContext db) =>
        new(new ApplicationJournalRepositoryAdapter(db), new ApplicationChartOfAccountRepositoryAdapter(db));

    private static async Task<(AccountingService svc, Guid cash, Guid revenue, Guid expense)> SeedAsync(ERPDbContext db)
    {
        var cash = Guid.NewGuid();
        var revenue = Guid.NewGuid();
        var expense = Guid.NewGuid();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                Id = cash, AccountId = cash, TenantId = Tenant,
                AccountCode = "1111", AccountName = "Cash", AccountType = "Asset",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = revenue, AccountId = revenue, TenantId = Tenant,
                AccountCode = "4000", AccountName = "Sales", AccountType = "Revenue",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = expense, AccountId = expense, TenantId = Tenant,
                AccountCode = "5000", AccountName = "Rent", AccountType = "Expense",
                IsPostingAllowed = true, IsActive = true
            });
        await db.SaveChangesAsync();
        return (Svc(db), cash, revenue, expense);
    }

    private static async Task PostCashReceiptAsync(AccountingService svc, Guid cash, Guid revenue, DateTime date, decimal amount)
    {
        var id = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashReceipt",
            JournalDate = date,
            CashOrBankAccountId = cash,
            CreditLines = [new CreateVoucherLineDto { AccountId = revenue, Amount = amount }]
        }, Tenant, User);
        Assert.Equal(id, await svc.ApproveJournalAsync(id, Tenant, Approver));
        Assert.Equal(id, await svc.PostJournalAsync(id, Tenant, Approver, allowPostWithoutApprove: false));
    }

    private static async Task PostCashPaymentAsync(AccountingService svc, Guid cash, Guid expense, DateTime date, decimal amount)
    {
        var id = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashPayment",
            JournalDate = date,
            CashOrBankAccountId = cash,
            DebitLines = [new CreateVoucherLineDto { AccountId = expense, Amount = amount }]
        }, Tenant, User);
        Assert.Equal(id, await svc.ApproveJournalAsync(id, Tenant, Approver));
        Assert.Equal(id, await svc.PostJournalAsync(id, Tenant, Approver, allowPostWithoutApprove: false));
    }

    [Fact]
    public async Task TrialBalance_Includes_Account_Metadata()
    {
        await using var db = Db();
        var (svc, cash, revenue, _) = await SeedAsync(db);
        var day = new DateTime(2026, 3, 15);
        await PostCashReceiptAsync(svc, cash, revenue, day, 100m);

        var tb = await svc.GetTrialBalanceAsync(Tenant, day.AddDays(-1), day.AddDays(1));
        Assert.Equal(100m, tb.TotalDebit);
        Assert.Equal(100m, tb.TotalCredit);
        Assert.Contains(tb.Lines, l => l.AccountCode == "1111" && l.AccountType == "Asset" && l.Debit == 100m);
        Assert.Contains(tb.Lines, l => l.AccountCode == "4000" && l.AccountType == "Revenue" && l.Credit == 100m);
    }

    [Fact]
    public async Task RunningTrialBalance_Opening_Plus_Period_Equals_Closing()
    {
        await using var db = Db();
        var (svc, cash, revenue, _) = await SeedAsync(db);
        await PostCashReceiptAsync(svc, cash, revenue, new DateTime(2026, 1, 10), 40m);
        await PostCashReceiptAsync(svc, cash, revenue, new DateTime(2026, 2, 10), 60m);

        var from = new DateTime(2026, 2, 1);
        var to = new DateTime(2026, 2, 28);
        var rtb = await svc.GetRunningTrialBalanceAsync(Tenant, from, to);
        var cashLine = rtb.Lines.Single(l => l.AccountCode == "1111");
        Assert.Equal(40m, cashLine.OpeningDebit);
        Assert.Equal(60m, cashLine.PeriodDebit);
        Assert.Equal(100m, cashLine.ClosingDebit);
        Assert.Equal(cashLine.OpeningBalance + (cashLine.PeriodDebit - cashLine.PeriodCredit), cashLine.ClosingBalance);
    }

    [Fact]
    public async Task ProfitAndLoss_Nets_Revenue_Minus_Expense()
    {
        await using var db = Db();
        var (svc, cash, revenue, expense) = await SeedAsync(db);
        var day = new DateTime(2026, 4, 1);
        await PostCashReceiptAsync(svc, cash, revenue, day, 200m);
        await PostCashPaymentAsync(svc, cash, expense, day, 50m);

        var pnl = await svc.GetProfitAndLossAsync(Tenant, day, day);
        Assert.Equal(200m, pnl.TotalRevenue);
        Assert.Equal(50m, pnl.TotalExpenses);
        Assert.Equal(150m, pnl.NetIncome);
    }

    [Fact]
    public async Task BalanceSheet_Plugs_NetIncome_Into_Equity()
    {
        await using var db = Db();
        var (svc, cash, revenue, expense) = await SeedAsync(db);
        var day = new DateTime(2026, 5, 1);
        await PostCashReceiptAsync(svc, cash, revenue, day, 200m);
        await PostCashPaymentAsync(svc, cash, expense, day, 50m);

        var bs = await svc.GetBalanceSheetAsync(Tenant, day);
        Assert.Equal(150m, bs.NetIncomePlug);
        Assert.Equal(150m, bs.TotalAssets); // cash 200 - 50
        Assert.Equal(bs.TotalAssets, bs.TotalLiabilitiesAndEquity);
        Assert.Contains(bs.Equity.Lines, l => l.AccountCode == "NI" && l.Balance == 150m);
    }

    [Fact]
    public async Task Ledger_Has_Running_Balance_And_Voucher_No()
    {
        await using var db = Db();
        var (svc, cash, revenue, _) = await SeedAsync(db);
        var day = new DateTime(2026, 6, 1);
        await PostCashReceiptAsync(svc, cash, revenue, day, 25m);

        var ledger = await svc.GetGeneralLedgerBalanceAsync(cash, Tenant, day, day);
        Assert.Single(ledger.Entries);
        Assert.False(string.IsNullOrWhiteSpace(ledger.Entries[0].JournalNumber));
        Assert.Equal(25m, ledger.Entries[0].RunningBalance);
        Assert.Equal(25m, ledger.ClosingBalance);
    }

    [Fact]
    public async Task DailyFinancial_Sums_Cash_And_Bank_Types()
    {
        await using var db = Db();
        var (svc, cash, revenue, expense) = await SeedAsync(db);
        var day = new DateTime(2026, 7, 1);
        await PostCashReceiptAsync(svc, cash, revenue, day, 80m);
        await PostCashPaymentAsync(svc, cash, expense, day, 30m);

        var daily = await svc.GetDailyFinancialAsync(Tenant, day);
        Assert.Equal(2, daily.VoucherCount);
        Assert.Equal(80m, daily.CashReceipts);
        Assert.Equal(30m, daily.CashPayments);
        Assert.Contains(daily.ByType, t => t.JournalType == "CashReceipt" && t.Count == 1);
    }

    [Fact]
    public async Task CashBankVoucherSummary_And_Monthly_Group_By_Type()
    {
        await using var db = Db();
        var (svc, cash, revenue, expense) = await SeedAsync(db);
        await PostCashReceiptAsync(svc, cash, revenue, new DateTime(2026, 8, 5), 40m);
        await PostCashPaymentAsync(svc, cash, expense, new DateTime(2026, 8, 6), 10m);

        var from = new DateTime(2026, 8, 1);
        var to = new DateTime(2026, 8, 31);
        var summary = await svc.GetCashBankVoucherSummaryAsync(Tenant, from, to);
        Assert.Contains(summary.Lines, l => l.JournalType == "CashReceipt" && l.Count == 1 && l.TotalAmount == 40m);
        Assert.Contains(summary.Lines, l => l.JournalType == "CashPayment" && l.Count == 1 && l.TotalAmount == 10m);

        var monthly = await svc.GetVoucherTypeMonthlySummaryAsync(Tenant, from, to);
        Assert.Contains(monthly.Lines, l => l.Period == "2026-08" && l.JournalType == "CashReceipt" && l.Count == 1);

        var cashLedger = await svc.GetCashBankLedgerSummaryAsync(Tenant, from, to);
        var cashLine = Assert.Single(cashLedger.Lines);
        Assert.Equal("1111", cashLine.AccountCode);
        Assert.Equal(30m, cashLine.ClosingBalance); // 40 in - 10 out
    }

    [Fact]
    public async Task CashFlow_Sums_Inflows_Outflows_And_Cash_Position()
    {
        await using var db = Db();
        var (svc, cash, revenue, expense) = await SeedAsync(db);
        await PostCashReceiptAsync(svc, cash, revenue, new DateTime(2026, 9, 5), 100m);
        await PostCashPaymentAsync(svc, cash, expense, new DateTime(2026, 9, 6), 40m);

        var from = new DateTime(2026, 9, 1);
        var to = new DateTime(2026, 9, 30);
        var cf = await svc.GetCashFlowAsync(Tenant, from, to);
        Assert.Equal(100m, cf.TotalInflows);
        Assert.Equal(40m, cf.TotalOutflows);
        Assert.Equal(60m, cf.NetChange);
        Assert.Equal(0m, cf.OpeningCash);
        Assert.Equal(60m, cf.ClosingCash);
        Assert.Contains(cf.Inflows, l => l.JournalType == "CashReceipt" && l.Amount == 100m);
        Assert.Contains(cf.Outflows, l => l.JournalType == "CashPayment" && l.Amount == 40m);
    }
}
