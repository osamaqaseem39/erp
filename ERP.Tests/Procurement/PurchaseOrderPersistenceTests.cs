using ERP.Application.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Procurement;

public class PurchaseOrderPersistenceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

    private static ERPDbContext Db(string name) => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(name)
        .Options);

    private static PurchaseOrderService Service(ERPDbContext db) =>
        new(new PurchaseOrderRepository(db), new PurchaseOrderDetailRepository(db));

    [Fact]
    public async Task Create_PersistsDetails_AndTotals()
    {
        await using var db = Db(Guid.NewGuid().ToString());
        var service = Service(db);

        var id = await service.CreateAsync(new CreatePurchaseOrderDto
        {
            VendorId = Vendor,
            OrderDate = DateTime.UtcNow.Date,
            ExpectedDeliveryDate = DateTime.UtcNow.Date.AddDays(7),
            Details =
            [
                new CreatePurchaseOrderDetailDto
                {
                    ItemId = Item,
                    Quantity = 4,
                    UnitPrice = 25,
                    Discount = 0,
                    TaxRate = 10
                }
            ]
        }, Tenant, User);

        var loaded = await service.GetByIdAsync(id, Tenant);
        Assert.NotNull(loaded);
        Assert.Single(loaded!.Details!);
        Assert.Equal(100m, loaded.SubTotal);
        Assert.Equal(10m, loaded.TaxAmount);
        Assert.Equal(110m, loaded.TotalAmount);
        Assert.Equal("Draft", loaded.Status);
    }

    [Fact]
    public async Task Approve_MovesDraftToIssued()
    {
        await using var db = Db(Guid.NewGuid().ToString());
        var service = Service(db);

        var id = await service.CreateAsync(new CreatePurchaseOrderDto
        {
            VendorId = Vendor,
            Details = [new CreatePurchaseOrderDetailDto { ItemId = Item, Quantity = 1, UnitPrice = 10 }]
        }, Tenant, User);

        Assert.False(await service.ApproveAsync(id, Tenant, User)); // ERP-FIX-6: creator cannot approve
        Assert.True(await service.ApproveAsync(id, Tenant, Approver));
        var loaded = await service.GetByIdAsync(id, Tenant);
        Assert.Equal("Issued", loaded!.Status);
        Assert.Equal(Approver.ToString(), loaded.ApprovedBy);
        Assert.False(await service.UpdateAsync(new UpdatePurchaseOrderDto { Id = id, VendorId = Vendor, Details = [] }, Tenant, User));
    }

    [Fact]
    public async Task Reject_CancelsOrder()
    {
        await using var db = Db(Guid.NewGuid().ToString());
        var service = Service(db);

        var id = await service.CreateAsync(new CreatePurchaseOrderDto
        {
            VendorId = Vendor,
            Details = [new CreatePurchaseOrderDetailDto { ItemId = Item, Quantity = 1, UnitPrice = 5 }]
        }, Tenant, User);

        Assert.True(await service.RejectAsync(id, Tenant, User));
        Assert.Equal("Cancelled", (await service.GetByIdAsync(id, Tenant))!.Status);
    }
}
