using ERP.Application.Core.Settings;
using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Admin;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class VoucherDefaultsTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static (AccountingService svc, CoreSettingsService settings) Svc(ERPDbContext db)
    {
        var settings = new CoreSettingsService(
            new SystemSettingRepository(db, new FixedTenant(Tenant)),
            new PlainProtector());
        var svc = new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db,
            settings);
        return (svc, settings);
    }

    [Fact]
    public async Task TypedVoucher_Remembers_And_Reuses_CashAccount_And_CostCenter()
    {
        await using var db = Db();
        var cash = Guid.NewGuid();
        var expense = Guid.NewGuid();
        var cc = Guid.NewGuid();
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
        db.CostCenters.Add(new CostCenter
        {
            Id = cc, CostCenterId = cc, TenantId = Tenant,
            CostCenterCode = "CC-1", CostCenterName = "Ops", IsActive = true
        });
        await db.SaveChangesAsync();

        var (svc, settings) = Svc(db);

        var firstId = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashPayment",
            JournalDate = DateTime.UtcNow.Date,
            CashOrBankAccountId = cash,
            DebitLines =
            [
                new CreateVoucherLineDto { AccountId = expense, Amount = 25m, CostCenterId = cc }
            ]
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, firstId);

        Assert.Equal(cash.ToString("D"), await settings.GetValueAsync(SettingsCatalog.LastCashOrBankAccountIdKey));
        Assert.Equal(cc.ToString("D"), await settings.GetValueAsync(SettingsCatalog.LastCostCenterIdKey));

        var defaults = await svc.GetVoucherDefaultsAsync(Tenant);
        Assert.Equal(cash, defaults.CashOrBankAccountId);
        Assert.Equal("1111", defaults.CashOrBankAccountCode);
        Assert.Equal(cc, defaults.CostCenterId);

        var secondId = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashPayment",
            JournalDate = DateTime.UtcNow.Date,
            // omit CashOrBankAccountId — should reuse last-used
            DebitLines =
            [
                new CreateVoucherLineDto { AccountId = expense, Amount = 10m }
            ]
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, secondId);

        var second = await db.Journals.SingleAsync(j => j.JournalId == secondId);
        Assert.Equal(cash, second.CashOrBankAccountId);

        var entries = await db.JournalEntries.Where(e => e.JournalId == secondId).ToListAsync();
        Assert.Contains(entries, e => e.AccountId == expense && e.CostCenterId == cc && e.Debit == 10m);
        Assert.Contains(entries, e => e.AccountId == cash && e.Credit == 10m);
    }

    [Fact]
    public async Task TypedVoucher_Without_Cash_And_No_Defaults_Fails()
    {
        await using var db = Db();
        var expense = Guid.NewGuid();
        db.ChartOfAccounts.Add(new ChartOfAccount
        {
            Id = expense, AccountId = expense, TenantId = Tenant,
            AccountCode = "5000", AccountName = "Rent", AccountType = "Expense",
            IsPostingAllowed = true, IsActive = true
        });
        await db.SaveChangesAsync();

        var (svc, _) = Svc(db);
        var id = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashPayment",
            JournalDate = DateTime.UtcNow.Date,
            DebitLines = [new CreateVoucherLineDto { AccountId = expense, Amount = 5m }]
        }, Tenant, User);
        Assert.Equal(Guid.Empty, id);
    }

    private sealed class FixedTenant(Guid id) : ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }

    private sealed class PlainProtector : ISecretProtector
    {
        public string Protect(string plaintext) => plaintext;
        public string Unprotect(string protectedValue) => protectedValue;
    }
}
