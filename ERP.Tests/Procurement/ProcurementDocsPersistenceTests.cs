using ERP.Application.Finance;
using ERP.Application.Inventory;
using ERP.Application.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Procurement;

public class ProcurementDocsPersistenceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Po = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task PurchaseRequisition_CreateApprove_Works()
    {
        await using var db = Db();
        var svc = new PurchaseRequisitionService(new PurchaseRequisitionRepository(db), new PurchaseRequisitionDetailRepository(db));
        var id = await svc.CreateAsync(new CreatePurchaseRequisitionDto
        {
            RequestedById = User,
            Details = [new CreatePurchaseRequisitionDetailDto { ItemId = Item, Quantity = 2, EstimatedUnitPrice = 15 }]
        }, Tenant, User);

        var loaded = await svc.GetByIdAsync(id, Tenant);
        Assert.NotNull(loaded);
        Assert.Single(loaded!.Details!);
        Assert.Equal(30m, loaded.TotalAmount);
        // FIX-6: creator (User) cannot approve their own PR; Approver is a different actor
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.Equal("Approved", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task GoodsReceiptNote_CreatePost_Works()
    {
        await using var db = Db();
        var stock = new StockService(new StockBalanceRepository(db));
        var svc = new GoodsReceiptNoteService(
            new GoodsReceiptNoteRepository(db),
            new GoodsReceiptNoteDetailRepository(db),
            new PurchaseOrderRepository(db),
            new PurchaseOrderDetailRepository(db),
            stock);
        var id = await svc.CreateAsync(new CreateGoodsReceiptNoteDto
        {
            VendorId = Vendor,
            PurchaseOrderId = Po,
            WarehouseId = Warehouse,
            Details = [new CreateGoodsReceiptNoteDetailDto { ItemId = Item, ReceivedQuantity = 3, UnitPrice = 10, TaxAmount = 3 }]
        }, Tenant, User);

        var loaded = await svc.GetByIdAsync(id, Tenant);
        Assert.NotNull(loaded);
        Assert.Single(loaded!.Details!);
        Assert.Equal(33m, loaded.Details![0].TotalAmount);
        Assert.True(await svc.ApproveAsync(id, Tenant, User));
        Assert.Equal("Posted", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.Equal(3m, await stock.GetStockLevelAsync(Item, Warehouse));
    }

    [Fact]
    public async Task PurchaseInvoice_CreateApprove_Works()
    {
        await using var db = Db();
        var bills = new BillService(new BillRepository(db));
        var svc = ERP.Tests.TestHelpers.ProcurementTestServices.CreatePurchaseInvoiceService(db, bills);
        var id = await svc.CreateAsync(new CreatePurchaseInvoiceDto
        {
            VendorId = Vendor,
            PurchaseOrderId = Po,
            Details = [new CreatePurchaseInvoiceDetailDto { ItemId = Item, Quantity = 2, UnitPrice = 50, TaxRate = 10 }]
        }, Tenant, User);

        var loaded = await svc.GetByIdAsync(id, Tenant);
        Assert.NotNull(loaded);
        Assert.Single(loaded!.Details!);
        Assert.Equal(100m, loaded.SubTotal);
        Assert.Equal(10m, loaded.TaxAmount);
        Assert.Equal(110m, loaded.TotalAmount);
        Assert.False(await svc.ApproveAsync(id, Tenant, User)); // ERP-FIX-6
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.Equal("Approved", (await svc.GetByIdAsync(id, Tenant))!.Status);

        var apBills = (await bills.GetByVendorAsync(Vendor, Tenant)).ToList();
        Assert.Single(apBills);
        Assert.Equal(110m, apBills[0].TotalAmount);
        Assert.Contains(id.ToString(), apBills[0].Notes);
    }
}
