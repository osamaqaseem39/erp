using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class BudgetVsActualTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AccountingService Acc(ERPDbContext db) =>
        new(new ApplicationJournalRepositoryAdapter(db), new ApplicationChartOfAccountRepositoryAdapter(db), db);

    [Fact]
    public async Task BudgetVsActual_Computes_Variance_From_Posted_GL()
    {
        await using var db = Db();
        var accounting = Acc(db);
        var expense = Guid.NewGuid();
        var cash = Guid.NewGuid();
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
        var budgetId = Guid.NewGuid();
        db.Budgets.Add(new Budget
        {
            Id = budgetId,
            BudgetId = budgetId,
            TenantId = Tenant,
            AccountId = expense,
            BudgetName = "FY26 Rent",
            BudgetPeriodStart = new DateTime(2026, 1, 1),
            BudgetPeriodEnd = new DateTime(2026, 12, 31),
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            BudgetAmount = 100m,
            Status = "Active",
            CurrencyCode = "USD"
        });
        await db.SaveChangesAsync();

        var jId = await accounting.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "CashPayment",
            JournalDate = new DateTime(2026, 3, 10),
            CashOrBankAccountId = cash,
            DebitLines = [new CreateVoucherLineDto { AccountId = expense, Amount = 70m }]
        }, Tenant, User);
        Assert.Equal(jId, await accounting.ApproveJournalAsync(jId, Tenant, Approver));
        Assert.Equal(jId, await accounting.PostJournalAsync(jId, Tenant, Approver, allowPostWithoutApprove: false));

        var report = await accounting.GetBudgetVsActualAsync(
            Tenant, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        var line = Assert.Single(report.Lines);
        Assert.Equal(100m, line.BudgetAmount);
        Assert.Equal(70m, line.ActualAmount);
        Assert.Equal(-30m, line.Variance);
        Assert.Equal(100m, report.TotalBudget);
        Assert.Equal(70m, report.TotalActual);
        Assert.Equal(-30m, report.TotalVariance);
    }
}
