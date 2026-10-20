using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class AccountsR2SummaryTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Supplier = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AccountingService Acc(ERPDbContext db) =>
        new(new ApplicationJournalRepositoryAdapter(db), new ApplicationChartOfAccountRepositoryAdapter(db));

    private static InvoiceService Inv(ERPDbContext db) =>
        new(new ApplicationInvoiceRepositoryAdapter(db));

    [Fact]
    public async Task DailyFinancial_And_CashBank_Summaries()
    {
        await using var db = Db();
        var svc = Acc(db);
        var cash = Guid.NewGuid();
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
                Id = expense, AccountId = expense, TenantId = Tenant,
                AccountCode = "5000", AccountName = "Rent", AccountType = "Expense",
                IsPostingAllowed = true, IsActive = true
            });
        await db.SaveChangesAsync();

        var day = new DateTime(2026, 7, 10);
        var id = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashPayment",
            JournalDate = day,
            CashOrBankAccountId = cash,
            DebitLines = [new CreateVoucherLineDto { AccountId = expense, Amount = 80m }]
        }, Tenant, User);
        await svc.ApproveJournalAsync(id, Tenant, Approver);
        await svc.PostJournalAsync(id, Tenant, Approver, allowPostWithoutApprove: false);

        var daily = await svc.GetDailyFinancialAsync(Tenant, day);
        Assert.Equal(1, daily.VoucherCount);
        Assert.Equal(80m, daily.CashPayments);

        var voucherSummary = await svc.GetCashBankVoucherSummaryAsync(Tenant, day, day);
        Assert.Contains(voucherSummary.Lines, l => l.JournalType == "CashPayment" && l.TotalAmount == 80m);

        var ledgerSummary = await svc.GetCashBankLedgerSummaryAsync(Tenant, day, day);
        Assert.Contains(ledgerSummary.Lines, l => l.AccountId == cash && l.ClosingBalance == -80m);

        var monthly = await svc.GetVoucherTypeMonthlySummaryAsync(Tenant, day, day);
        Assert.Contains(monthly.Lines, l => l.JournalType == "CashPayment" && l.Period == "2026-07" && l.Count == 1);
    }

    [Fact]
    public async Task Aging_Buckets_And_Partner_Summary()
    {
        await using var db = Db();
        var asOf = new DateTime(2026, 8, 31);
        db.Invoices.Add(new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceId = Guid.NewGuid(),
            TenantId = Tenant,
            CustomerId = Customer,
            InvoiceNumber = "INV-1",
            InvoiceDate = asOf.AddDays(-45),
            DueDate = asOf.AddDays(-40),
            TotalAmount = 100m,
            Balance = 100m,
            Status = "Issued",
            CurrencyCode = "USD"
        });
        db.Invoices.Add(new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceId = Guid.NewGuid(),
            TenantId = Tenant,
            CustomerId = Customer,
            InvoiceNumber = "INV-2",
            InvoiceDate = asOf,
            DueDate = asOf.AddDays(10),
            TotalAmount = 50m,
            Balance = 50m,
            Status = "Issued",
            CurrencyCode = "USD"
        });
        db.Bills.Add(new Bill
        {
            Id = Guid.NewGuid(),
            BillId = Guid.NewGuid(),
            TenantId = Tenant,
            SupplierId = Supplier,
            VendorId = Supplier,
            BillNumber = "BILL-1",
            BillDate = asOf.AddDays(-10),
            DueDate = asOf.AddDays(-5),
            TotalAmount = 70m,
            Balance = 70m,
            Status = "Issued",
            CurrencyCode = "USD"
        });
        await db.SaveChangesAsync();

        var inv = Inv(db);
        var ar = await inv.GetAgedReceivablesAsync(Tenant, asOf);
        Assert.Equal(150m, ar.TotalBalance);
        Assert.Contains(ar.Buckets, b => b.Label == "Current" && b.Amount == 50m);
        Assert.Contains(ar.Buckets, b => b.Label == "31-60" && b.Amount == 100m);

        var ap = await inv.GetAgedPayablesAsync(Tenant, asOf);
        Assert.Equal(70m, ap.TotalBalance);
        Assert.Contains(ap.Buckets, b => b.Label == "1-30" && b.Amount == 70m);

        var partners = await inv.GetPartnerBalanceSummaryAsync(Tenant, asOf);
        Assert.Equal(150m, partners.TotalReceivables);
        Assert.Equal(70m, partners.TotalPayables);
        Assert.Contains(partners.Customers, c => c.PartnerId == Customer && c.OpenDocuments == 2);
        Assert.Contains(partners.Suppliers, s => s.PartnerId == Supplier && s.Balance == 70m);
    }

    [Fact]
    public async Task CashFlow_From_Posted_Cash_Bank_Vouchers()
    {
        await using var db = Db();
        var svc = Acc(db);
        var cash = Guid.NewGuid();
        var bank = Guid.NewGuid();
        var expense = Guid.NewGuid();
        var income = Guid.NewGuid();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                Id = cash, AccountId = cash, TenantId = Tenant,
                AccountCode = "1111", AccountName = "Cash", AccountType = "Asset",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = bank, AccountId = bank, TenantId = Tenant,
                AccountCode = "1120", AccountName = "Bank", AccountType = "Asset",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = expense, AccountId = expense, TenantId = Tenant,
                AccountCode = "5000", AccountName = "Rent", AccountType = "Expense",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = income, AccountId = income, TenantId = Tenant,
                AccountCode = "4000", AccountName = "Sales", AccountType = "Revenue",
                IsPostingAllowed = true, IsActive = true
            });
        await db.SaveChangesAsync();

        var day = new DateTime(2026, 10, 5);
        var payId = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashPayment",
            JournalDate = day,
            CashOrBankAccountId = cash,
            DebitLines = [new CreateVoucherLineDto { AccountId = expense, Amount = 80m }]
        }, Tenant, User);
        await svc.ApproveJournalAsync(payId, Tenant, Approver);
        await svc.PostJournalAsync(payId, Tenant, Approver, allowPostWithoutApprove: false);

        var recvId = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "BankReceipt",
            JournalDate = day,
            CashOrBankAccountId = bank,
            CreditLines = [new CreateVoucherLineDto { AccountId = income, Amount = 200m }]
        }, Tenant, User);
        await svc.ApproveJournalAsync(recvId, Tenant, Approver);
        await svc.PostJournalAsync(recvId, Tenant, Approver, allowPostWithoutApprove: false);

        var draftId = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashReceipt",
            JournalDate = day,
            CashOrBankAccountId = cash,
            CreditLines = [new CreateVoucherLineDto { AccountId = income, Amount = 50m }]
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, draftId);

        var report = await svc.GetCashFlowAsync(Tenant, day, day);
        Assert.Equal(200m, report.TotalInflows);
        Assert.Equal(80m, report.TotalOutflows);
        Assert.Equal(120m, report.NetChange);
        Assert.Contains(report.Outflows, l => l.JournalType == "CashPayment" && l.Amount == 80m);
        Assert.Contains(report.Inflows, l => l.JournalType == "BankReceipt" && l.Amount == 200m);
        Assert.DoesNotContain(report.Inflows, l => l.JournalType == "CashReceipt");
    }
}
