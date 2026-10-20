using ERP.Application.Finance;
using ERP.Application.Interfaces.Finance;
using ERP.Domain.Core;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class OpeningBalancePostTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Create_Post_CreatesPostedJournal()
    {
        await using var db = Db();
        var cashId = Guid.NewGuid();
        var equityId = Guid.NewGuid();
        var fyId = Guid.NewGuid();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                Id = cashId, AccountId = cashId, TenantId = Tenant,
                AccountCode = "1000", AccountName = "Cash", AccountType = "Asset", IsPostingAllowed = true
            },
            new ChartOfAccount
            {
                Id = equityId, AccountId = equityId, TenantId = Tenant,
                AccountCode = "3000", AccountName = "Equity", AccountType = "Equity", IsPostingAllowed = true
            });
        db.FiscalYears.Add(new FiscalYear
        {
            Id = fyId, FiscalYearId = fyId, TenantId = Tenant,
            Name = "FY2026", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = "Active"
        });
        await db.SaveChangesAsync();

        var accounting = new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db);
        var svc = new OpeningBalanceService(db, accounting);

        var created = await svc.CreateAsync(new CreateOpeningBalanceDto
        {
            FiscalYearId = fyId,
            ReferenceNo = "OB-001",
            AsOfDate = new DateTime(2026, 1, 1),
            Lines =
            [
                new CreateOpeningBalanceLineDto { AccountId = cashId, Debit = 1000, Credit = 0, LineNumber = 1 },
                new CreateOpeningBalanceLineDto { AccountId = equityId, Debit = 0, Credit = 1000, LineNumber = 2 }
            ]
        }, Tenant, User);

        Assert.Equal("Draft", created.Status);

        var posted = await svc.PostAsync(created.Id, Tenant, User);
        Assert.Equal("Posted", posted.Status);
        Assert.NotNull(posted.JournalId);
        Assert.NotEqual(Guid.Empty, posted.JournalId);

        var journal = await accounting.GetJournalByIdAsync(posted.JournalId!.Value, Tenant);
        Assert.NotNull(journal);
        Assert.Equal("Posted", journal!.Status);
    }
}
