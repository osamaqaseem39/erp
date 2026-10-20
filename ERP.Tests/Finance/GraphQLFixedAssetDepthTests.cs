using ERP.Api.GraphQL.Finance;
using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class GraphQLFixedAssetDepthTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AccountingService Acc(ERPDbContext db) =>
        new(new ApplicationJournalRepositoryAdapter(db), new ApplicationChartOfAccountRepositoryAdapter(db), db);

    private static FixedAssetService Svc(ERPDbContext db) => new(db, Acc(db));

    private static FinanceQueries Queries(FixedAssetService svc) =>
        new(null!, null!, null!, null!, svc);

    private static FinanceMutations Mutations(FixedAssetService svc) =>
        new(null!, null!, null!, null!, null!, svc);

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
    public void Surface_ExposesDepreciationDepth()
    {
        var queries = typeof(FinanceQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(FinanceMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetDepreciationEntries", queries);
        Assert.Contains("GetDepreciationEntriesByPeriod", queries);
        Assert.Contains("GetAvailableDepreciationMethods", queries);
        Assert.Contains("GetAssetNetBookValue", queries);
        Assert.Contains("GetAssetValuationReport", queries);
        Assert.Contains("GetDepreciationSchedule", queries);
        Assert.Contains("GetAssetCategories", queries);
        Assert.Contains("CalculateDepreciation", mutations);
        Assert.Contains("UndoDisposal", mutations);
    }

    [Fact]
    public async Task GraphQL_Methods_Calculate_Entries_After_Post()
    {
        await using var db = Db();
        var svc = Svc(db);
        var gl = await SeedGlAsync(db);
        var queries = Queries(svc);
        var mutations = Mutations(svc);

        var methods = (await queries.GetAvailableDepreciationMethods()).ToList();
        Assert.NotEmpty(methods);

        var id = await mutations.AddFixedAsset(new CreateFixedAssetDto
        {
            AssetName = "Server Rack",
            AssetType = "Equipment",
            GLAccountId = gl,
            PurchasePrice = 12_000m,
            PurchaseDate = new DateTime(2026, 1, 1)
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var amount = await mutations.CalculateDepreciation(
            id, new DateTime(2026, 2, 1), new DateTime(2026, 2, 28), Tenant);
        Assert.True(amount > 0);

        Assert.True(await mutations.PostDepreciationForPeriod(
            new DateTime(2026, 2, 1), new DateTime(2026, 2, 28), Tenant, User));

        var entries = await queries.GetDepreciationEntries(id, Tenant);
        Assert.Single(entries);
        Assert.Equal(200m, entries[0].DepreciationAmount);

        var byPeriod = await queries.GetDepreciationEntriesByPeriod(
            Tenant, new DateTime(2026, 2, 1), new DateTime(2026, 2, 28));
        Assert.Contains(byPeriod, e => e.AssetId == id);

        var nbv = await queries.GetAssetNetBookValue(id, new DateTime(2026, 2, 28), Tenant);
        Assert.Equal(11_800m, nbv);
    }
}
