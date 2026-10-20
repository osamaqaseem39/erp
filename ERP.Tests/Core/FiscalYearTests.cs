using ERP.Application.Core;
using ERP.Application.Finance;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Core;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Core;

public class FiscalYearTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Cash = Guid.NewGuid();
    private static readonly Guid Revenue = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static FiscalYearService Fy(ERPDbContext db) => new(new FiscalYearRepository(db));

    private static AccountingService Acct(ERPDbContext db) =>
        new(new ApplicationJournalRepositoryAdapter(db), new ApplicationChartOfAccountRepositoryAdapter(db), db);

    [Fact]
    public async Task Create_RejectsOverlap_AndAllowsNonOverlap()
    {
        await using var db = Db();
        var svc = Fy(db);

        var id1 = await svc.CreateAsync(new CreateFiscalYearDto
        {
            Name = "FY2025",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31)
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id1);

        var overlap = await svc.CreateAsync(new CreateFiscalYearDto
        {
            Name = "Overlap",
            StartDate = new DateTime(2025, 6, 1),
            EndDate = new DateTime(2026, 5, 31)
        }, Tenant, User);
        Assert.Equal(Guid.Empty, overlap);

        var id2 = await svc.CreateAsync(new CreateFiscalYearDto
        {
            Name = "FY2026",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31)
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id2);
    }

    [Fact]
    public async Task Close_ThenDelete_BlockedWhenJournalReferences()
    {
        await using var db = Db();
        var fySvc = Fy(db);
        var fyId = await fySvc.CreateAsync(new CreateFiscalYearDto
        {
            Name = "FY2025",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31)
        }, Tenant, User);

        var acct = Acct(db);
        var journalId = await CreateBalancedJournalAsync(acct, new DateTime(2025, 3, 15));
        Assert.Equal(journalId, await acct.PostJournalAsync(journalId, Tenant, User));

        var posted = await acct.GetJournalByIdAsync(journalId, Tenant);
        Assert.Equal(fyId, posted!.FinancialYearId);

        Assert.True(await fySvc.CloseAsync(fyId, Tenant, User));
        var closed = await fySvc.GetByIdAsync(fyId, Tenant);
        Assert.Equal("Closed", closed!.Status);

        Assert.False(await fySvc.DeleteAsync(fyId, Tenant));
    }

    [Fact]
    public async Task PostJournal_FailsOutsideActiveFiscalYear_WhenConfigured()
    {
        await using var db = Db();
        var fySvc = Fy(db);
        await fySvc.CreateAsync(new CreateFiscalYearDto
        {
            Name = "FY2025",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31)
        }, Tenant, User);

        var acct = Acct(db);
        var outside = await CreateBalancedJournalAsync(acct, new DateTime(2024, 6, 1));
        Assert.Equal(Guid.Empty, await acct.PostJournalAsync(outside, Tenant, User));

        var inside = await CreateBalancedJournalAsync(acct, new DateTime(2025, 6, 1));
        Assert.Equal(inside, await acct.PostJournalAsync(inside, Tenant, User));
    }

    [Fact]
    public async Task PostJournal_AllowedWhenNoFiscalYearsConfigured()
    {
        await using var db = Db();
        var acct = Acct(db);
        var journalId = await CreateBalancedJournalAsync(acct, DateTime.UtcNow.Date);
        Assert.Equal(journalId, await acct.PostJournalAsync(journalId, Tenant, User));
        var posted = await acct.GetJournalByIdAsync(journalId, Tenant);
        Assert.Null(posted!.FinancialYearId);
    }

    [Fact]
    public async Task TenantFailClosed_EmptyTenantReturnsNothing()
    {
        await using var db = Db();
        db.FiscalYears.Add(new FiscalYear
        {
            Id = Guid.NewGuid(),
            FiscalYearId = Guid.NewGuid(),
            TenantId = Tenant,
            Name = "FY",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            Status = "Active"
        });
        await db.SaveChangesAsync();

        var svc = Fy(db);
        Assert.Empty(await svc.GetAllAsync(Guid.Empty));
        Assert.Null(await svc.GetByIdAsync(Guid.NewGuid(), Guid.Empty));
    }

    private static async Task<Guid> CreateBalancedJournalAsync(AccountingService acct, DateTime date)
    {
        var journalId = await acct.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "Journal",
            JournalDate = date,
            Description = "FY test",
            Status = "Draft"
        }, Tenant, User);

        await acct.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = Cash,
            Debit = 50,
            Credit = 0
        }, journalId, Tenant, User);

        await acct.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = Revenue,
            Debit = 0,
            Credit = 50
        }, journalId, Tenant, User);

        return journalId;
    }
}
