using ERP.Application.SCM.Adapters;
using ERP.Application.SCM.Services;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class WarehouseTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid Company = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static WarehouseService Svc(ERPDbContext db) =>
        new(new ApplicationWarehouseRepositoryAdapter(new WarehouseRepository(db)));

    [Fact]
    public async Task Create_SetsTenant_And_CrossTenantGetFails()
    {
        await using var db = Db();
        var svc = Svc(db);

        var created = await svc.CreateAsync(new CreateWarehouseDto
        {
            CompanyId = Company,
            WarehouseCode = "WH-A1",
            WarehouseName = "Alpha",
            City = "Karachi",
            IsActive = true
        }, TenantA, "user");

        Assert.Equal(TenantA, created.TenantId);
        Assert.Equal("WH-A1", created.WarehouseCode);

        Assert.NotNull(await svc.GetByIdAsync(created.Id, TenantA));
        Assert.Null(await svc.GetByIdAsync(created.Id, TenantB));
        Assert.Null(await svc.GetByIdAsync(created.Id, Guid.Empty));
    }

    [Fact]
    public async Task List_Search_FailClosed_EmptyTenant_And_ScopedByTenant()
    {
        await using var db = Db();
        db.Warehouses.AddRange(
            new Warehouse
            {
                Id = Guid.NewGuid(),
                TenantId = TenantA,
                CompanyId = Company,
                WarehouseCode = "A-1",
                WarehouseName = "Alpha Store",
                Name = "Alpha Store",
                City = "Lahore",
                IsActive = true
            },
            new Warehouse
            {
                Id = Guid.NewGuid(),
                TenantId = TenantB,
                CompanyId = Company,
                WarehouseCode = "B-1",
                WarehouseName = "Beta Store",
                Name = "Beta Store",
                City = "Lahore",
                IsActive = true
            });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var a = (await svc.GetAllAsync(TenantA, Company)).ToList();
        Assert.Single(a);
        Assert.Equal("A-1", a[0].WarehouseCode);

        Assert.Empty(await svc.GetAllAsync(Guid.Empty, Company));
        Assert.Empty(await svc.SearchAsync("Alpha", Guid.Empty, Company));

        var search = (await svc.SearchAsync("Alpha", TenantA, Company)).ToList();
        Assert.Single(search);
        Assert.Empty(await svc.SearchAsync("Alpha", TenantB, Company));
    }

    [Fact]
    public async Task Update_Activate_Delete_Reject_CrossTenant_And_EmptyTenant()
    {
        await using var db = Db();
        var svc = Svc(db);
        var created = await svc.CreateAsync(new CreateWarehouseDto
        {
            CompanyId = Company,
            WarehouseCode = "WH-X",
            WarehouseName = "Cross",
            IsActive = true
        }, TenantA, "user");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateAsync(new CreateWarehouseDto
            {
                CompanyId = Company,
                WarehouseCode = "Z",
                WarehouseName = "Z"
            }, Guid.Empty, "user"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.UpdateAsync(new UpdateWarehouseDto
            {
                Id = created.Id,
                WarehouseName = "Hacked"
            }, TenantB, "user"));

        Assert.False(await svc.ActivateAsync(created.Id, TenantB, "user"));
        Assert.False(await svc.DeactivateAsync(created.Id, Guid.Empty, "user"));
        Assert.True(await svc.DeactivateAsync(created.Id, TenantA, "user"));
        Assert.True(await svc.ActivateAsync(created.Id, TenantA, "user"));

        Assert.False(await svc.DeleteAsync(created.Id, TenantB));
        Assert.False(await svc.DeleteAsync(created.Id, Guid.Empty));
        Assert.True(await svc.DeleteAsync(created.Id, TenantA));
    }
}
