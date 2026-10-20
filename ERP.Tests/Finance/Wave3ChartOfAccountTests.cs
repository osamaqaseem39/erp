using ERP.Application.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class Wave3ChartOfAccountTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AccountingService Svc(ERPDbContext db) =>
        new(new ApplicationJournalRepositoryAdapter(db), new ApplicationChartOfAccountRepositoryAdapter(db));

    [Fact]
    public async Task Create_List_Update_Delete_Works()
    {
        await using var db = Db();
        var svc = Svc(db);

        var cashId = await svc.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = "Asset",
            IsActive = true,
            IsPostingAllowed = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, cashId);

        Assert.Equal(Guid.Empty, await svc.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "1000",
            AccountName = "Dup",
            AccountType = "Asset"
        }, Tenant, User));

        var childId = await svc.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "1100",
            AccountName = "Petty Cash",
            AccountType = "Asset",
            ParentAccountId = cashId
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, childId);

        var all = (await svc.GetAllChartOfAccountsAsync(Tenant, accountType: "Asset")).ToList();
        Assert.Equal(2, all.Count);

        var updated = await svc.UpdateChartOfAccountAsync(new UpdateChartOfAccountDto
        {
            AccountId = childId,
            AccountName = "Petty Cash Box"
        }, Tenant, User);
        Assert.Equal(childId, updated);
        Assert.Equal("Petty Cash Box", (await svc.GetChartOfAccountByIdAsync(childId, Tenant))!.AccountName);

        Assert.False(await svc.DeleteChartOfAccountAsync(cashId, Tenant)); // has child
        Assert.True(await svc.DeleteChartOfAccountAsync(childId, Tenant));
        Assert.True(await svc.DeleteChartOfAccountAsync(cashId, Tenant));
    }

    [Fact]
    public async Task Delete_Blocked_When_JournalEntries_Exist()
    {
        await using var db = Db();
        var svc = Svc(db);
        var cash = await svc.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = "Asset"
        }, Tenant, User);
        var revenue = await svc.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "4000",
            AccountName = "Revenue",
            AccountType = "Revenue"
        }, Tenant, User);

        var jv = await svc.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "Sales",
            JournalDate = DateTime.UtcNow.Date
        }, Tenant, User);
        await svc.AddJournalEntryAsync(new CreateJournalEntryDto { AccountId = cash, Debit = 10, Credit = 0 }, jv, Tenant, User);
        await svc.AddJournalEntryAsync(new CreateJournalEntryDto { AccountId = revenue, Debit = 0, Credit = 10 }, jv, Tenant, User);
        await svc.PostJournalAsync(jv, Tenant, User);

        Assert.False(await svc.DeleteChartOfAccountAsync(cash, Tenant));
        Assert.False(await svc.DeleteChartOfAccountAsync(revenue, Tenant));
    }
}
