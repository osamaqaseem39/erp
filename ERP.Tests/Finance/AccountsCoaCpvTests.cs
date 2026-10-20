using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class AccountsCoaCpvTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Creator = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AccountingService Svc(ERPDbContext db) =>
        new(new ApplicationJournalRepositoryAdapter(db), new ApplicationChartOfAccountRepositoryAdapter(db));

    [Fact]
    public async Task CoA_Levels_Computed_And_Filtered()
    {
        await using var db = Db();
        var svc = Svc(db);

        var l1 = await svc.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "1000",
            AccountName = "Assets",
            AccountType = "Asset",
            IsPostingAllowed = false
        }, Tenant, Creator);
        Assert.NotEqual(Guid.Empty, l1);

        var l2 = await svc.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "1100",
            AccountName = "Current Assets",
            AccountType = "Asset",
            ParentAccountId = l1,
            IsPostingAllowed = false
        }, Tenant, Creator);
        Assert.NotEqual(Guid.Empty, l2);

        var l3 = await svc.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "1110",
            AccountName = "Cash in Hand",
            AccountType = "Asset",
            ParentAccountId = l2,
            IsPostingAllowed = false
        }, Tenant, Creator);

        var l4 = await svc.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "1111",
            AccountName = "Petty Cash",
            AccountType = "Asset",
            ParentAccountId = l3,
            Remarks = "Office petty cash",
            IsPostingAllowed = true
        }, Tenant, Creator);

        var level2 = (await svc.GetAllChartOfAccountsAsync(Tenant, level: 2)).ToList();
        Assert.Single(level2);
        Assert.Equal(2, level2[0].Level);
        Assert.Equal("1100", level2[0].AccountCode);

        var posting = (await svc.GetAllChartOfAccountsAsync(Tenant, postingOnly: true)).ToList();
        Assert.Contains(posting, a => a.AccountId == l4);
        Assert.Equal("Office petty cash", posting.First(a => a.AccountId == l4).Remarks);
    }

    [Fact]
    public async Task Seed_Creates_L1_Once()
    {
        await using var db = Db();
        var svc = Svc(db);
        Assert.Equal(5, await svc.SeedDefaultChartOfAccountsAsync(Tenant, Creator));
        Assert.Equal(0, await svc.SeedDefaultChartOfAccountsAsync(Tenant, Creator));
        var l1 = (await svc.GetAllChartOfAccountsAsync(Tenant, level: 1)).ToList();
        Assert.Equal(5, l1.Count);
    }

    [Fact]
    public async Task CashPayment_Builds_Debit_And_Credit_Then_Approve_Post()
    {
        await using var db = Db();
        var svc = Svc(db);

        var cashId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                Id = cashId, AccountId = cashId, TenantId = Tenant,
                AccountCode = "1111", AccountName = "Petty Cash", AccountType = "Asset",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = expenseId, AccountId = expenseId, TenantId = Tenant,
                AccountCode = "5100", AccountName = "Website Expenses", AccountType = "Expense",
                IsPostingAllowed = true, IsActive = true
            });
        await db.SaveChangesAsync();

        var journalId = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashPayment",
            JournalDate = DateTime.UtcNow.Date,
            Description = "Amjad gave 500 for website",
            CashOrBankAccountId = cashId,
            PaidBy = "Amjad",
            ReceivedBy = "Aslam",
            DebitLines =
            [
                new CreateCashPaymentLineDto
                {
                    AccountId = expenseId,
                    Amount = 500m,
                    Narration = "Website expenses"
                }
            ]
        }, Tenant, Creator);

        Assert.NotEqual(Guid.Empty, journalId);
        var draft = await svc.GetJournalByIdAsync(journalId, Tenant);
        Assert.Equal("Draft", draft!.Status);
        Assert.StartsWith("CPV-", draft.JournalNumber);
        Assert.Equal(2, draft.JournalEntries!.Count);
        Assert.Equal(500m, draft.TotalDebit);
        Assert.Equal(500m, draft.TotalCredit);

        // creator cannot approve
        Assert.Equal(Guid.Empty, await svc.ApproveJournalAsync(journalId, Tenant, Creator));

        Assert.Equal(journalId, await svc.ApproveJournalAsync(journalId, Tenant, Approver));
        var approved = await svc.GetJournalByIdAsync(journalId, Tenant);
        Assert.Equal("Approved", approved!.Status);

        Assert.Equal(journalId, await svc.PostJournalAsync(journalId, Tenant, Approver, allowPostWithoutApprove: false));
        var posted = await svc.GetJournalByIdAsync(journalId, Tenant);
        Assert.Equal("Posted", posted!.Status);
        Assert.True(posted.IsLocked);
    }

    [Fact]
    public async Task CashReceipt_And_BankPayment_AutoBalance_Sides()
    {
        await using var db = Db();
        var svc = Svc(db);

        var cashId = Guid.NewGuid();
        var bankId = Guid.NewGuid();
        var incomeId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                Id = cashId, AccountId = cashId, TenantId = Tenant,
                AccountCode = "CASH", AccountName = "Cash", AccountType = "Asset",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = bankId, AccountId = bankId, TenantId = Tenant,
                AccountCode = "BANK", AccountName = "Bank", AccountType = "Asset",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = incomeId, AccountId = incomeId, TenantId = Tenant,
                AccountCode = "INC", AccountName = "Sales Income", AccountType = "Revenue",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = expenseId, AccountId = expenseId, TenantId = Tenant,
                AccountCode = "EXP", AccountName = "Office Expense", AccountType = "Expense",
                IsPostingAllowed = true, IsActive = true
            });
        await db.SaveChangesAsync();

        var crv = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashReceipt",
            JournalDate = DateTime.UtcNow.Date,
            CashOrBankAccountId = cashId,
            CreditLines =
            [
                new CreateVoucherLineDto { AccountId = incomeId, Amount = 200m, Narration = "Cash sale" }
            ]
        }, Tenant, Creator);
        Assert.NotEqual(Guid.Empty, crv);
        var crvDto = await svc.GetJournalByIdAsync(crv, Tenant);
        Assert.StartsWith("CRV-", crvDto!.JournalNumber);
        Assert.Equal(200m, crvDto.TotalDebit);
        Assert.Contains(crvDto.JournalEntries!, e => e.AccountId == cashId && e.Debit == 200m);
        Assert.Contains(crvDto.JournalEntries!, e => e.AccountId == incomeId && e.Credit == 200m);

        var bpv = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "BankPayment",
            JournalDate = DateTime.UtcNow.Date,
            CashOrBankAccountId = bankId,
            ChequeNumber = "CHK-9",
            DebitLines =
            [
                new CreateVoucherLineDto { AccountId = expenseId, Amount = 75m }
            ]
        }, Tenant, Creator);
        Assert.NotEqual(Guid.Empty, bpv);
        var bpvDto = await svc.GetJournalByIdAsync(bpv, Tenant);
        Assert.StartsWith("BPV-", bpvDto!.JournalNumber);
        Assert.Equal("CHK-9", bpvDto.ChequeNumber);
        Assert.Contains(bpvDto.JournalEntries!, e => e.AccountId == bankId && e.Credit == 75m);
        Assert.Contains(bpvDto.JournalEntries!, e => e.AccountId == expenseId && e.Debit == 75m);
    }

    [Fact]
    public async Task Unpost_Returns_To_Draft()
    {
        await using var db = Db();
        var svc = Svc(db);
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                Id = a, AccountId = a, TenantId = Tenant, AccountCode = "A", AccountName = "A",
                AccountType = "Asset", IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = b, AccountId = b, TenantId = Tenant, AccountCode = "B", AccountName = "B",
                AccountType = "Expense", IsPostingAllowed = true, IsActive = true
            });
        await db.SaveChangesAsync();

        var id = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashPayment",
            JournalDate = DateTime.UtcNow.Date,
            CashOrBankAccountId = a,
            DebitLines = [new CreateVoucherLineDto { AccountId = b, Amount = 10m }]
        }, Tenant, Creator);

        Assert.Equal(id, await svc.ApproveJournalAsync(id, Tenant, Approver));
        Assert.Equal(id, await svc.PostJournalAsync(id, Tenant, Approver, allowPostWithoutApprove: false));
        Assert.Equal("Posted", (await svc.GetJournalByIdAsync(id, Tenant))!.Status);

        Assert.Equal(id, await svc.UnpostJournalAsync(id, Tenant, Approver));
        var unposted = await svc.GetJournalByIdAsync(id, Tenant);
        Assert.Equal("Draft", unposted!.Status);
        Assert.False(unposted.IsLocked);
        Assert.Null(unposted.PostedAt);
    }

    [Fact]
    public async Task CostCenter_And_AccountDefaults_RoundTrip()
    {
        await using var db = Db();
        var accounts = new ApplicationChartOfAccountRepositoryAdapter(db);
        var ccSvc = new CostCenterService(new ERP.Infrastructure.Repositories.Finance.CostCenterRepository(db));
        var defSvc = new AccountDefaultService(db, accounts);

        var cashId = await new AccountingService(
            new ApplicationJournalRepositoryAdapter(db), accounts)
            .AddChartOfAccountAsync(new CreateChartOfAccountDto
            {
                AccountCode = "CASH",
                AccountName = "Petty Cash",
                AccountType = "Asset",
                IsPostingAllowed = true
            }, Tenant, Creator);

        var ccId = await ccSvc.CreateAsync(new CreateCostCenterDto
        {
            CostCenterCode = "HO",
            CostCenterName = "Head Office"
        }, Tenant, Creator);
        Assert.NotEqual(Guid.Empty, ccId);

        var list = (await defSvc.UpsertAsync(new UpsertAccountDefaultsDto
        {
            Defaults = [new UpsertAccountDefaultDto { Purpose = AccountPurposes.Cash, AccountId = cashId }]
        }, Tenant, Creator)).ToList();

        Assert.Contains(list, d => d.Purpose == AccountPurposes.Cash && d.AccountId == cashId);
        var byPurpose = await defSvc.GetByPurposeAsync(AccountPurposes.Cash, Tenant);
        Assert.NotNull(byPurpose);
        Assert.Equal(cashId, byPurpose!.AccountId);
    }
}
