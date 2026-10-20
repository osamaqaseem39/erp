using ERP.Application.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class Wave3JournalTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Cash = Guid.NewGuid();
    private static readonly Guid Revenue = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AccountingService Svc(ERPDbContext db) =>
        new(new ApplicationJournalRepositoryAdapter(db), new ApplicationChartOfAccountRepositoryAdapter(db));

    [Fact]
    public async Task CreateEntries_Post_RequiresBalance_ThenReverse()
    {
        await using var db = Db();
        var svc = Svc(db);

        var journalId = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "Sales",
            JournalDate = DateTime.UtcNow.Date,
            Description = "Wave3 JV",
            Status = "Draft"
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, journalId);

        await svc.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = Cash,
            Debit = 100,
            Credit = 0,
            Narration = "Cash"
        }, journalId, Tenant, User);

        // unbalanced — post must fail
        Assert.Equal(Guid.Empty, await svc.PostJournalAsync(journalId, Tenant, User));

        await svc.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = Revenue,
            Debit = 0,
            Credit = 100,
            Narration = "Revenue"
        }, journalId, Tenant, User);

        var before = await svc.GetJournalByIdAsync(journalId, Tenant);
        Assert.Equal(100m, before!.TotalDebit);
        Assert.Equal(100m, before.TotalCredit);
        Assert.Equal(2, before.JournalEntries!.Count);

        Assert.Equal(journalId, await svc.PostJournalAsync(journalId, Tenant, User));
        var posted = await svc.GetJournalByIdAsync(journalId, Tenant);
        Assert.Equal("Posted", posted!.Status);
        Assert.True(posted.IsLocked);

        // cannot add after post
        Assert.Equal(Guid.Empty, await svc.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = Cash,
            Debit = 1,
            Credit = 0
        }, journalId, Tenant, User));

        Assert.Equal(journalId, await svc.ReverseJournalAsync(journalId, Tenant, User, "test reverse"));
        var reversed = await svc.GetJournalByIdAsync(journalId, Tenant);
        Assert.Equal("Reversed", reversed!.Status);

        var (journals, count) = await svc.SearchJournalsAsync(new JournalQuery
        {
            TenantId = Tenant,
            Status = "Posted",
            PageNumber = 1,
            PageSize = 20
        });
        Assert.True(count >= 1);
        Assert.Contains(journals, j => j.ReferenceNo != null && j.ReferenceNo.StartsWith("REV-"));
    }

    [Fact]
    public async Task TrialBalance_SumsPostedEntries()
    {
        await using var db = Db();
        var svc = Svc(db);
        var journalId = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "Adjustment",
            JournalDate = DateTime.UtcNow.Date
        }, Tenant, User);
        await svc.AddJournalEntryAsync(new CreateJournalEntryDto { AccountId = Cash, Debit = 50, Credit = 0 }, journalId, Tenant, User);
        await svc.AddJournalEntryAsync(new CreateJournalEntryDto { AccountId = Revenue, Debit = 0, Credit = 50 }, journalId, Tenant, User);
        await svc.PostJournalAsync(journalId, Tenant, User);

        var tb = await svc.GetTrialBalanceAsync(Tenant, DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(1));
        Assert.Equal(50m, tb.TotalDebit);
        Assert.Equal(50m, tb.TotalCredit);
        Assert.Contains(tb.Lines, l => !string.IsNullOrEmpty(l.AccountCode));
    }
}
