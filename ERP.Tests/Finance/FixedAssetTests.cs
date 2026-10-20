using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class FixedAssetTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AccountingService Acc(ERPDbContext db) =>
        new(new ApplicationJournalRepositoryAdapter(db), new ApplicationChartOfAccountRepositoryAdapter(db), db);

    private static FixedAssetService Svc(ERPDbContext db) => new(db, Acc(db));

    private static async Task<Guid> SeedGlAsync(ERPDbContext db)
    {
        var id = Guid.NewGuid();
        var expense = Guid.NewGuid();
        var accum = Guid.NewGuid();
        db.ChartOfAccounts.AddRange(
            new ChartOfAccount
            {
                Id = id, AccountId = id, TenantId = Tenant,
                AccountCode = "1500", AccountName = "Fixed Assets", AccountType = "Asset",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = expense, AccountId = expense, TenantId = Tenant,
                AccountCode = "5700", AccountName = "Depreciation Expense", AccountType = "Expense",
                IsPostingAllowed = true, IsActive = true
            },
            new ChartOfAccount
            {
                Id = accum, AccountId = accum, TenantId = Tenant,
                AccountCode = "1590", AccountName = "Accumulated Depreciation", AccountType = "Asset",
                IsPostingAllowed = true, IsActive = true
            });
        db.AccountDefaults.AddRange(
            new AccountDefault
            {
                Id = Guid.NewGuid(), AccountDefaultId = Guid.NewGuid(), TenantId = Tenant,
                Purpose = AccountPurposes.DepreciationExpense, AccountId = expense, IsActive = true
            },
            new AccountDefault
            {
                Id = Guid.NewGuid(), AccountDefaultId = Guid.NewGuid(), TenantId = Tenant,
                Purpose = AccountPurposes.AccumulatedDepreciation, AccountId = accum, IsActive = true
            });
        await db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Create_Get_Update_Dispose_Undo()
    {
        await using var db = Db();
        var svc = Svc(db);
        var gl = await SeedGlAsync(db);

        var id = await svc.AddAssetAsync(new CreateFixedAssetDto
        {
            AssetCode = "TRUCK-1",
            AssetName = "Delivery Truck",
            AssetType = "Vehicle",
            GLAccountId = gl,
            PurchasePrice = 40_000m,
            PurchaseDate = new DateTime(2026, 1, 15)
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var loaded = await svc.GetAssetByIdAsync(id, Tenant);
        Assert.Equal("TRUCK-1", loaded!.AssetCode);
        Assert.Equal(40_000m, loaded.PurchasePrice);
        Assert.Equal(40_000m, loaded.NetBookValue);
        Assert.Equal("Vehicle", loaded.AssetType);

        var updated = await svc.UpdateAssetAsync(new UpdateFixedAssetDto
        {
            AssetId = id,
            AssetName = "Delivery Truck 2"
        }, Tenant, User);
        Assert.Equal(id, updated);
        Assert.Equal("Delivery Truck 2", (await svc.GetAssetByIdAsync(id, Tenant))!.AssetName);

        Assert.True(await svc.DisposeAssetAsync(id, 5_000m, new DateTime(2026, 10, 1), Tenant, User));
        var disposed = await svc.GetAssetByIdAsync(id, Tenant);
        Assert.Equal("Disposed", disposed!.Status);
        Assert.Equal(5_000m, disposed.BookValue);
        Assert.False(disposed.IsActive);

        Assert.True(await svc.UndoDisposalAsync(id, Tenant, User));
        var active = await svc.GetAssetByIdAsync(id, Tenant);
        Assert.Equal("Active", active!.Status);
        Assert.True(active.IsActive);
    }

    [Fact]
    public async Task Search_And_StraightLine_Depreciation_Post()
    {
        await using var db = Db();
        var svc = Svc(db);
        var gl = await SeedGlAsync(db);

        var id = await svc.AddAssetAsync(new CreateFixedAssetDto
        {
            AssetName = "Server Rack",
            AssetType = "Equipment",
            GLAccountId = gl,
            PurchasePrice = 12_000m,
            PurchaseDate = new DateTime(2026, 1, 1)
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var (assets, total) = await svc.SearchAssetsAsync(new FixedAssetQuery
        {
            TenantId = Tenant,
            Category = "Equipment",
            PageNumber = 1,
            PageSize = 20
        });
        Assert.Equal(1, total);
        Assert.Contains(assets, a => a.AssetId == id);

        var month = await svc.CalculateStraightLineDepreciationAsync(
            12_000m, 0m, 60, new DateTime(2026, 1, 1),
            new DateTime(2026, 2, 1), new DateTime(2026, 2, 28));
        Assert.Equal(200m, month);

        Assert.True(await svc.PostDepreciationForPeriodAsync(
            new DateTime(2026, 2, 1), new DateTime(2026, 2, 28), Tenant, User));

        var after = await svc.GetAssetByIdAsync(id, Tenant);
        Assert.Equal(11_800m, after!.BookValue);
        Assert.Equal(200m, after.AccumulatedDepreciation);

        var entries = (await svc.GetDepreciationEntriesAsync(id, Tenant)).ToList();
        Assert.Single(entries);
        Assert.Equal(200m, entries[0].DepreciationAmount);
        Assert.Equal("Posted", entries[0].Status);
        Assert.NotNull(entries[0].PostedAt);

        var dep = await db.DepreciationEntries.FirstAsync(d => d.AssetId == id);
        Assert.NotNull(dep.PostedJournalId);
        var journal = await db.Journals.Include(j => j.JournalEntries)
            .FirstAsync(j => j.JournalId == dep.PostedJournalId);
        Assert.Equal("Posted", journal.Status);
        Assert.Equal(200m, journal.JournalEntries.Sum(e => e.Debit));
        Assert.Equal(200m, journal.JournalEntries.Sum(e => e.Credit));

        // Second post same period is no-op
        Assert.False(await svc.PostDepreciationForPeriodAsync(
            new DateTime(2026, 2, 1), new DateTime(2026, 2, 28), Tenant, User));
    }

    [Fact]
    public async Task Rejects_Duplicate_Code_And_Missing_Gl()
    {
        await using var db = Db();
        var svc = Svc(db);
        var gl = await SeedGlAsync(db);

        var first = await svc.AddAssetAsync(new CreateFixedAssetDto
        {
            AssetCode = "DUP",
            AssetName = "A",
            AssetType = "Equipment",
            GLAccountId = gl,
            PurchasePrice = 100m,
            PurchaseDate = new DateTime(2026, 3, 1)
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, first);

        Assert.Equal(Guid.Empty, await svc.AddAssetAsync(new CreateFixedAssetDto
        {
            AssetCode = "DUP",
            AssetName = "B",
            AssetType = "Equipment",
            GLAccountId = gl,
            PurchasePrice = 100m,
            PurchaseDate = new DateTime(2026, 3, 1)
        }, Tenant, User));

        Assert.Equal(Guid.Empty, await svc.AddAssetAsync(new CreateFixedAssetDto
        {
            AssetName = "No GL",
            AssetType = "Equipment",
            PurchasePrice = 100m,
            PurchaseDate = new DateTime(2026, 3, 1)
        }, Tenant, User));
    }

    [Fact]
    public async Task Empty_Tenant_FailClosed()
    {
        await using var db = Db();
        var gl = await SeedGlAsync(db);
        var svc = Svc(db);

        Assert.Equal(Guid.Empty, await svc.AddAssetAsync(new CreateFixedAssetDto
        {
            AssetCode = "FA-X",
            AssetName = "X",
            AssetType = "Equipment",
            GLAccountId = gl,
            PurchasePrice = 100m,
            PurchaseDate = new DateTime(2026, 1, 1)
        }, Guid.Empty, User));

        var id = await svc.AddAssetAsync(new CreateFixedAssetDto
        {
            AssetCode = "FA-OK",
            AssetName = "Ok",
            AssetType = "Equipment",
            GLAccountId = gl,
            PurchasePrice = 100m,
            PurchaseDate = new DateTime(2026, 1, 1)
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        Assert.Null(await svc.GetAssetByIdAsync(id, Guid.Empty));
        var (assets, total) = await svc.SearchAssetsAsync(new FixedAssetQuery { TenantId = Guid.Empty });
        Assert.Empty(assets);
        Assert.Equal(0, total);
        Assert.Empty(await svc.GetDepreciationEntriesAsync(id, Guid.Empty));
        Assert.False(await svc.PostDepreciationForPeriodAsync(
            new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), Guid.Empty, User));
    }
}
