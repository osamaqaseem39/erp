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

public class ProcurementWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Creator = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task PurchaseOrder_IllegalTransition_AndSelfApprove_Rejected()
    {
        await using var db = Db();
        var svc = new PurchaseOrderService(new PurchaseOrderRepository(db), new PurchaseOrderDetailRepository(db));
        var id = await svc.CreateAsync(new CreatePurchaseOrderDto
        {
            VendorId = Vendor,
            Status = "Issued",
            Details = [new CreatePurchaseOrderDetailDto { ItemId = Item, Quantity = 1, UnitPrice = 10 }]
        }, Tenant, Creator);

        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.ApproveAsync(id, Tenant, Creator));
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.Equal("Issued", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.False(await svc.UpdateAsync(new UpdatePurchaseOrderDto
        {
            Id = id,
            VendorId = Vendor,
            Status = "Draft",
            Details = [new CreatePurchaseOrderDetailDto { ItemId = Item, Quantity = 1, UnitPrice = 10 }]
        }, Tenant, Approver));

        Assert.False(await svc.ApproveAsync(id, Tenant, Approver)); // Issued → Issued illegal
        Assert.True(await svc.RejectAsync(id, Tenant, Approver));
        Assert.Equal("Cancelled", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.RejectAsync(id, Tenant, Approver));
        Assert.False(await svc.DeleteAsync(id, Tenant));
    }

    [Fact]
    public async Task PurchaseInvoice_PostImmutability_AndSelfApprove()
    {
        await using var db = Db();
        var svc = ERP.Tests.TestHelpers.ProcurementTestServices.CreatePurchaseInvoiceService(
            db,
            new BillService(new BillRepository(db)));

        var id = await svc.CreateAsync(new CreatePurchaseInvoiceDto
        {
            VendorId = Vendor,
            Status = "Approved",
            Details = [new CreatePurchaseInvoiceDetailDto { ItemId = Item, Quantity = 1, UnitPrice = 20, TaxRate = 0 }]
        }, Tenant, Creator);

        Assert.Equal("Pending", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await svc.ApproveAsync(id, Tenant, Creator));
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));

        Assert.False(await svc.UpdateAsync(new UpdatePurchaseInvoiceDto
        {
            Id = id,
            VendorId = Vendor,
            Status = "Pending",
            Details = [new CreatePurchaseInvoiceDetailDto { ItemId = Item, Quantity = 1, UnitPrice = 20 }]
        }, Tenant, Approver));

        Assert.False(await svc.DeleteAsync(id, Tenant));
        Assert.False(await svc.ApproveAsync(id, Tenant, Approver));
    }

    [Fact]
    public async Task Grn_UpdateAfterPost_Rejected_SelfPostAllowed()
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
            WarehouseId = Warehouse,
            Status = "Posted",
            Details = [new CreateGoodsReceiptNoteDetailDto { ItemId = Item, ReceivedQuantity = 2, UnitPrice = 5 }]
        }, Tenant, Creator);

        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
        Assert.True(await svc.ApproveAsync(id, Tenant, Creator)); // post may be by receiver
        Assert.Equal("Posted", (await svc.GetByIdAsync(id, Tenant))!.Status);

        Assert.False(await svc.UpdateAsync(new UpdateGoodsReceiptNoteDto
        {
            Id = id,
            VendorId = Vendor,
            WarehouseId = Warehouse,
            Status = "Draft",
            Details = [new CreateGoodsReceiptNoteDetailDto { ItemId = Item, ReceivedQuantity = 2, UnitPrice = 5 }]
        }, Tenant, Creator));

        Assert.False(await svc.ApproveAsync(id, Tenant, Creator));
        Assert.False(await svc.RejectAsync(id, Tenant, Creator));
        Assert.False(await svc.DeleteAsync(id, Tenant));
    }
}
