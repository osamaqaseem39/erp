using ERP.Application.SCM.Services;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Inventory;

public class UnitOfMeasureTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static UnitOfMeasureService Svc(ERPDbContext db) =>
        new(new UnitOfMeasureRepository(db));

    [Fact]
    public async Task Create_SetsTenant_And_CrossTenantGetFails()
    {
        await using var db = Db();
        var svc = Svc(db);

        var created = await svc.CreateAsync(new UnitOfMeasureDto
        {
            Code = "EA",
            Name = "Each",
            ConversionFactor = 1,
            IsActive = true
        }, TenantA, "user");

        Assert.Equal(TenantA, created.TenantId);
        Assert.Equal("EA", created.Code);

        Assert.NotNull(await svc.GetByIdAsync(created.Id, TenantA));
        Assert.Null(await svc.GetByIdAsync(created.Id, TenantB));
        Assert.Null(await svc.GetByIdAsync(created.Id, Guid.Empty));
    }

    [Fact]
    public async Task List_FailClosed_EmptyTenant_And_ScopedByTenant()
    {
        await using var db = Db();
        db.Set<UnitOfMeasure>().AddRange(
            new UnitOfMeasure
            {
                Id = Guid.NewGuid(),
                TenantId = TenantA,
                UnitCode = "KG",
                UnitName = "Kilogram",
                IsActive = true
            },
            new UnitOfMeasure
            {
                Id = Guid.NewGuid(),
                TenantId = TenantB,
                UnitCode = "LB",
                UnitName = "Pound",
                IsActive = true
            });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var a = (await svc.GetAllAsync(TenantA)).ToList();
        Assert.Single(a);
        Assert.Equal("KG", a[0].Code);

        Assert.Empty(await svc.GetAllAsync(Guid.Empty));
    }

    [Fact]
    public async Task Update_And_Delete_Reject_CrossTenant_And_EmptyTenant()
    {
        await using var db = Db();
        var svc = Svc(db);
        var created = await svc.CreateAsync(new UnitOfMeasureDto
        {
            Code = "BOX",
            Name = "Box",
            ConversionFactor = 1,
            IsActive = true
        }, TenantA, "user");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateAsync(new UnitOfMeasureDto { Code = "Z", Name = "Z" }, Guid.Empty, "user"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.UpdateAsync(new UnitOfMeasureDto
            {
                Id = created.Id,
                Code = "BOX",
                Name = "Hacked"
            }, TenantB, "user"));

        Assert.False(await svc.DeleteAsync(created.Id, TenantB));
        Assert.False(await svc.DeleteAsync(created.Id, Guid.Empty));
        Assert.True(await svc.DeleteAsync(created.Id, TenantA));
    }
}
