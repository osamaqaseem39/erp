using ERP.Api.GraphQL.Core;
using ERP.Application.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Core;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Core;

public class GraphQLFiscalYearTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static FiscalYearService Svc(ERPDbContext db) => new(new FiscalYearRepository(db));

    [Fact]
    public void Surface_ExposesFiscalYearOps()
    {
        var queries = typeof(CoreModuleQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(CoreModuleQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(CoreModuleMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(CoreModuleMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetFiscalYear", queries);
        Assert.Contains("GetFiscalYears", queries);
        Assert.Contains("GetActiveFiscalYearCovering", queries);
        Assert.Contains("CreateFiscalYear", mutations);
        Assert.Contains("UpdateFiscalYear", mutations);
        Assert.Contains("CloseFiscalYear", mutations);
        Assert.Contains("DeleteFiscalYear", mutations);
    }

    [Fact]
    public async Task GraphQL_Create_List_Cover_Close()
    {
        await using var db = Db();
        var svc = Svc(db);
        var queries = new CoreModuleQueries();
        var mutations = new CoreModuleMutations();

        var id = await mutations.CreateFiscalYear(
            svc,
            new CreateFiscalYearDto
            {
                Name = "FY2025",
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 12, 31)
            },
            Tenant,
            User);
        Assert.NotEqual(Guid.Empty, id);

        var listed = (await queries.GetFiscalYears(svc, Tenant, status: "Active")).ToList();
        Assert.Contains(listed, f => f.FiscalYearId == id);

        var covering = await queries.GetActiveFiscalYearCovering(svc, Tenant, new DateTime(2025, 6, 15));
        Assert.NotNull(covering);
        Assert.Equal(id, covering!.FiscalYearId);

        Assert.Null(await queries.GetActiveFiscalYearCovering(svc, Tenant, new DateTime(2024, 1, 1)));

        Assert.True(await mutations.CloseFiscalYear(svc, id, Tenant, User));
        var closed = await queries.GetFiscalYear(svc, id, Tenant);
        Assert.Equal("Closed", closed!.Status);
    }
}
