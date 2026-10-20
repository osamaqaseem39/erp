using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class CostCenterTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Repo_Get_List_Code_FailClosed_And_Isolate()
    {
        await using var db = Db();
        var id = Guid.NewGuid();
        db.CostCenters.Add(new CostCenter
        {
            Id = id,
            CostCenterId = id,
            TenantId = TenantA,
            CostCenterCode = "CC-A",
            CostCenterName = "Ops A",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var repo = new CostCenterRepository(db);

        Assert.NotNull(await repo.GetByIdAsync(id, TenantA));
        Assert.Null(await repo.GetByIdAsync(id, TenantB));
        Assert.Null(await repo.GetByIdAsync(id, Guid.Empty));

        Assert.Single(await repo.GetAllAsync(TenantA));
        Assert.Empty(await repo.GetAllAsync(TenantB));
        Assert.Empty(await repo.GetAllAsync(Guid.Empty));

        Assert.NotNull(await repo.GetByCodeAsync("CC-A", TenantA));
        Assert.Null(await repo.GetByCodeAsync("CC-A", TenantB));
        Assert.Null(await repo.GetByCodeAsync("CC-A", Guid.Empty));

        Assert.Equal(Guid.Empty, await repo.CreateAsync(new CostCenter
        {
            TenantId = Guid.Empty,
            CostCenterCode = "X",
            CostCenterName = "No tenant"
        }));
    }

    [Fact]
    public async Task Service_Mutations_FailClosed_On_Empty_Tenant()
    {
        await using var db = Db();
        var svc = new CostCenterService(new CostCenterRepository(db));

        Assert.Equal(Guid.Empty, await svc.CreateAsync(new CreateCostCenterDto
        {
            CostCenterCode = "CC1",
            CostCenterName = "One"
        }, Guid.Empty, Guid.NewGuid()));

        Assert.Null(await svc.GetByIdAsync(Guid.NewGuid(), Guid.Empty));
        Assert.Empty(await svc.GetAllAsync(Guid.Empty));
        Assert.False(await svc.DeleteAsync(Guid.NewGuid(), Guid.Empty));
        Assert.False(await svc.SoftDeactivateAsync(Guid.NewGuid(), Guid.Empty));
    }
}
