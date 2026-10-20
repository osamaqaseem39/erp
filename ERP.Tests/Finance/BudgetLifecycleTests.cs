using ERP.Application.Core.Settings;
using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class BudgetLifecycleTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Create_UpdateIgnoresStatus_Activate_EnforceBlocksPost()
    {
        await using var db = Db();
        var expenseId = Guid.NewGuid();
        var cashId = Guid.NewGuid();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                Id = expenseId, AccountId = expenseId, TenantId = Tenant,
                AccountCode = "5000", AccountName = "Expense", AccountType = "Expense", IsPostingAllowed = true
            },
            new ChartOfAccount
            {
                Id = cashId, AccountId = cashId, TenantId = Tenant,
                AccountCode = "1000", AccountName = "Cash", AccountType = "Asset", IsPostingAllowed = true
            });
        await db.SaveChangesAsync();

        var budgets = new BudgetService(db);
        var created = await budgets.CreateAsync(new CreateBudgetDto
        {
            BudgetName = "Opex",
            GLAccountId = expenseId,
            FiscalYear = 2026,
            BudgetAmount = 100,
            Status = "Active" // ignored — always Draft
        }, Tenant, User);
        Assert.Equal("Draft", created.Status);

        var updated = await budgets.UpdateAsync(new UpdateBudgetDto
        {
            Id = created.Id,
            BudgetName = "Opex FY26",
            Status = "Closed" // ignored
        }, Tenant, User);
        Assert.Equal("Draft", updated.Status);
        Assert.Equal("Opex FY26", updated.BudgetName);

        Assert.True(await budgets.ActivateAsync(created.Id, Tenant, User));
        Assert.Equal("Active", (await budgets.GetAsync(created.Id, Tenant))!.Status);

        var settings = new TestCoreSettings();
        await settings.SetValueAsync(SettingsCatalog.EnforceBudgetKey, "true");
        var accounting = new AccountingService(
            new ApplicationJournalRepositoryAdapter(db, settings: settings, budgetEnforcement: new BudgetEnforcementService(db, settings)),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db);

        var journalId = await accounting.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "General",
            JournalDate = new DateTime(2026, 6, 1),
            Description = "over budget"
        }, Tenant, User);
        await accounting.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = expenseId, Debit = 150, Credit = 0
        }, journalId, Tenant, User);
        await accounting.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = cashId, Debit = 0, Credit = 150
        }, journalId, Tenant, User);

        await Assert.ThrowsAsync<ValidationException>(() =>
            accounting.PostJournalAsync(journalId, Tenant, User, allowPostWithoutApprove: true));
    }
}
