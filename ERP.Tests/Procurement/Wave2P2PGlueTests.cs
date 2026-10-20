using ERP.Application.Finance;
using ERP.Application.Inventory;
using ERP.Application.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Procurement;

public class Wave2P2PGlueTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task GrnApprove_ReceivesStock_AndUpdatesPoReceivedQty()
    {
        await using var db = Db();

        var poRepo = new PurchaseOrderRepository(db);
        var poDetailRepo = new PurchaseOrderDetailRepository(db);
        var poId = await poRepo.CreateAsync(new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            VendorId = Vendor,
            PurchaseOrderNumber = "PO-W2-1",
            OrderDate = DateTime.UtcNow,
            Status = "Issued",
            CreatedBy = User.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await poDetailRepo.CreateAsync(new PurchaseOrderDetail
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            POId = poId,
            ItemId = Item,
            QuantityOrdered = 10,
            UnitPrice = 5,
            NetAmount = 50,
            ReceivedQuantity = 0,
            CreatedBy = User.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        var stock = new StockService(new StockBalanceRepository(db));
        var grn = new GoodsReceiptNoteService(
            new GoodsReceiptNoteRepository(db),
            new GoodsReceiptNoteDetailRepository(db),
            poRepo,
            poDetailRepo,
            stock);

        var grnId = await grn.CreateAsync(new CreateGoodsReceiptNoteDto
        {
            VendorId = Vendor,
            PurchaseOrderId = poId,
            WarehouseId = Warehouse,
            Details = [new CreateGoodsReceiptNoteDetailDto { ItemId = Item, ReceivedQuantity = 4, UnitPrice = 5 }]
        }, Tenant, User);

        Assert.True(await grn.ApproveAsync(grnId, Tenant, User));
        Assert.Equal("Posted", (await grn.GetByIdAsync(grnId, Tenant))!.Status);

        Assert.Equal(4m, await stock.GetStockLevelAsync(Item, Warehouse));

        var poLines = (await poDetailRepo.GetByPurchaseOrderIdAsync(poId, Tenant)).ToList();
        Assert.Equal(4m, poLines[0].ReceivedQuantity);
        Assert.Equal("PartiallyReceived", (await poRepo.GetByIdAsync(poId, Tenant))!.Status);

        var grn2 = await grn.CreateAsync(new CreateGoodsReceiptNoteDto
        {
            VendorId = Vendor,
            PurchaseOrderId = poId,
            WarehouseId = Warehouse,
            Details = [new CreateGoodsReceiptNoteDetailDto { ItemId = Item, ReceivedQuantity = 6, UnitPrice = 5 }]
        }, Tenant, User);
        Assert.True(await grn.ApproveAsync(grn2, Tenant, User));
        Assert.Equal(10m, await stock.GetStockLevelAsync(Item, Warehouse));
        Assert.Equal(10m, (await poDetailRepo.GetByPurchaseOrderIdAsync(poId, Tenant)).First().ReceivedQuantity);
        Assert.Equal("Completed", (await poRepo.GetByIdAsync(poId, Tenant))!.Status);
    }

    [Fact]
    public async Task PurchaseInvoice_Reject_VoidsApBill()
    {
        await using var db = Db();
        var bills = new BillService(new BillRepository(db));
        var pi = ERP.Tests.TestHelpers.ProcurementTestServices.CreatePurchaseInvoiceService(db, bills);
        var creator = Guid.NewGuid();
        var approver = Guid.NewGuid();

        var id = await pi.CreateAsync(new CreatePurchaseInvoiceDto
        {
            VendorId = Vendor,
            InvoiceNumber = "PI-CANCEL-1",
            Details =
            [
                new CreatePurchaseInvoiceDetailDto { ItemId = Item, Quantity = 2, UnitPrice = 25, TaxRate = 0 }
            ]
        }, Tenant, creator);
        Assert.True(await pi.ApproveAsync(id, Tenant, approver));
        Assert.Single(await bills.GetByVendorAsync(Vendor, Tenant));

        Assert.True(await pi.RejectAsync(id, Tenant, approver));
        Assert.Equal("Cancelled", (await pi.GetByIdAsync(id, Tenant))!.Status);

        var ap = Assert.Single(await bills.GetByVendorAsync(Vendor, Tenant));
        Assert.Equal("Cancelled", ap.Status);
        Assert.Equal(0m, ap.Balance);
    }

    [Fact]
    public async Task PurchaseInvoice_Reject_Fails_When_Ap_Has_Payments()
    {
        await using var db = Db();
        var bills = new BillService(new BillRepository(db));
        var pi = ERP.Tests.TestHelpers.ProcurementTestServices.CreatePurchaseInvoiceService(db, bills);
        var creator = Guid.NewGuid();
        var approver = Guid.NewGuid();

        var id = await pi.CreateAsync(new CreatePurchaseInvoiceDto
        {
            VendorId = Vendor,
            InvoiceNumber = "PI-PAID-1",
            Details =
            [
                new CreatePurchaseInvoiceDetailDto { ItemId = Item, Quantity = 1, UnitPrice = 80, TaxRate = 0 }
            ]
        }, Tenant, creator);
        Assert.True(await pi.ApproveAsync(id, Tenant, approver));
        var ap = Assert.Single(await bills.GetByVendorAsync(Vendor, Tenant));
        Assert.True(await bills.ApplyCreditToBillAsync(ap.Id, 20m, Tenant));

        Assert.False(await pi.RejectAsync(id, Tenant, approver));
        Assert.Equal("Approved", (await pi.GetByIdAsync(id, Tenant))!.Status);
        Assert.Equal("PartiallyPaid", (await bills.GetByIdAsync(ap.Id, Tenant))!.Status);
    }
}
