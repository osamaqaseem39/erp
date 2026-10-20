using ERP.Application.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class WarehouseEfTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid Company = Guid.NewGuid();

    private sealed class FixedTenant(Guid id) : ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task ScmRepo_FailsClosed_On_Empty_Tenant_And_Isolates()
    {
        await using var db = Db();
        var repo = new WarehouseRepository(db);
        var id = (await repo.AddAsync(new Warehouse
        {
            TenantId = TenantB,
            CompanyId = Company,
            WarehouseCode = "WH-B",
            WarehouseName = "B",
            Name = "B"
        })).Id;

        Assert.Null(await repo.GetByIdAsync(id, TenantA));
        Assert.Null(await repo.GetByIdAsync(id, Guid.Empty));
        Assert.Empty(await repo.GetAllAsync(Guid.Empty, Company));
        Assert.False(await repo.DeleteAsync(id, TenantA));
        Assert.NotNull(await repo.GetByIdAsync(id, TenantB));
    }

    [Fact]
    public async Task InventoryService_Activate_Deactivate_TenantScoped()
    {
        await using var db = Db();
        db.Warehouses.Add(new Warehouse
        {
            Id = Guid.NewGuid(),
            TenantId = TenantA,
            CompanyId = Company,
            WarehouseCode = "WH-A",
            WarehouseName = "A",
            Name = "A",
            IsActive = true
        });
        db.Warehouses.Add(new Warehouse
        {
            Id = Guid.NewGuid(),
            TenantId = TenantB,
            CompanyId = Company,
            WarehouseCode = "WH-B",
            WarehouseName = "B",
            Name = "B",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var svc = new WarehouseService(new WarehouseRepositoryAdapter(db, new FixedTenant(TenantA)));
        Assert.Single(await svc.GetAllAsync());
        Assert.Single(await svc.GetActiveAsync());

        var a = (await svc.GetAllAsync()).Single();
        Assert.True(await svc.DeactivateAsync(a.Id));
        Assert.Empty(await svc.GetActiveAsync());
        Assert.True(await svc.ActivateAsync(a.Id));
        Assert.Single(await svc.GetActiveAsync());

        // Other tenant's warehouse is invisible
        var otherId = db.Warehouses.Single(w => w.TenantId == TenantB).Id;
        Assert.Null(await svc.GetByIdAsync(otherId));
        Assert.False(await svc.DeactivateAsync(otherId));
    }

    [Fact]
    public async Task InventoryService_Create_Uses_TenantProvider()
    {
        await using var db = Db();
        var svc = new WarehouseService(new WarehouseRepositoryAdapter(db, new FixedTenant(TenantA)));
        var id = await svc.CreateAsync(new CreateWarehouseDto
        {
            CompanyId = Company,
            WarehouseCode = "WH-NEW",
            WarehouseName = "New Dock",
            City = "Karachi"
        });
        Assert.NotEqual(Guid.Empty, id);
        var row = await db.Warehouses.SingleAsync(w => w.Id == id);
        Assert.Equal(TenantA, row.TenantId);
        Assert.Equal("New Dock", row.Name);
    }
}
