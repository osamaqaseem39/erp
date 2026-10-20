using ERP.Application.Finance;
using ERP.Application.Inventory;
using ERP.Application.Procurement;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Procurement;

public class PurchaseReturnDebitNoteTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static (PurchaseReturnService Returns, PurchaseInvoiceService Pi, PurchaseDebitNoteService Debit, BillService Bills) Services(
        ERPDbContext db)
    {
        var stock = new StockService(new StockBalanceRepository(db));
        var bills = new BillService(new BillRepository(db));
        var pi = ERP.Tests.TestHelpers.ProcurementTestServices.CreatePurchaseInvoiceService(db, bills);
        var debit = new PurchaseDebitNoteService(
            new PurchaseDebitNoteRepository(db),
            new PurchaseDebitNoteLineRepository(db),
            new PurchaseInvoiceRepository(db),
            bills,
            accounting: null,
            db: null,
            stock);
        var returns = new PurchaseReturnService(new PurchaseReturnRepository(db), stock, debit);
        return (returns, pi, debit, bills);
    }

    private static async Task<Guid> CreateAndApprovePiAsync(PurchaseInvoiceService pi, decimal qty, decimal price)
    {
        var id = await pi.CreateAsync(new CreatePurchaseInvoiceDto
        {
            VendorId = Vendor,
            InvoiceNumber = "PI-PR-1",
            Details =
            [
                new CreatePurchaseInvoiceDetailDto
                {
                    ItemId = Item,
                    Quantity = qty,
                    UnitPrice = price,
                    TaxRate = 0
                }
            ]
        }, Tenant, User);
        Assert.True(await pi.ApproveAsync(id, Tenant, Approver));
        return id;
    }

    [Fact]
    public async Task Approve_Complete_IssuesStock_And_PostsPurchaseDebitNote()
    {
        await using var db = Db();
        db.Warehouses.Add(new Warehouse
        {
            Id = Warehouse,
            TenantId = Tenant,
            WarehouseCode = "WH1",
            WarehouseName = "Main"
        });
        db.StockBalances.Add(new StockBalance
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ItemId = Item,
            WarehouseId = Warehouse,
            CurrentQuantity = 10m,
            ReservedQuantity = 0m
        });
        await db.SaveChangesAsync();

        var (returns, pi, debit, bills) = Services(db);
        var piId = await CreateAndApprovePiAsync(pi, qty: 2, price: 40m);

        var id = await returns.CreateAsync(new CreatePurchaseReturnDto
        {
            ReturnNumber = "PR-100",
            ReturnDate = DateTime.UtcNow.Date,
            VendorId = Vendor,
            PurchaseInvoiceId = piId,
            WarehouseId = Warehouse,
            Reason = "Defective",
            Lines =
            [
                new CreatePurchaseReturnLineDto { ItemId = Item, Quantity = 2, UnitPrice = 40m }
            ]
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var created = await returns.GetByIdAsync(id, Tenant);
        Assert.Equal("Pending", created!.Status);
        Assert.Equal(80m, created.TotalAmount);
        Assert.Single(created.Lines);

        Assert.False(await returns.CompleteAsync(id, Tenant, User)); // must approve first
        Assert.True(await returns.ApproveAsync(id, Tenant, User));
        Assert.True(await returns.CompleteAsync(id, Tenant, User));

        var done = await returns.GetByIdAsync(id, Tenant);
        Assert.Equal("Completed", done!.Status);
        Assert.NotNull(done.DebitNoteId);

        var stockRow = await db.StockBalances.FirstOrDefaultAsync(s =>
            s.ItemId == Item && s.WarehouseId == Warehouse);
        Assert.NotNull(stockRow);
        Assert.Equal(8m, stockRow!.CurrentQuantity);

        var dn = await debit.GetByIdAsync(done.DebitNoteId!.Value, Tenant);
        Assert.NotNull(dn);
        Assert.Equal("Posted", dn!.Status);
        Assert.Equal(80m, dn.TotalAmount);
        Assert.Equal(piId, dn.OriginalPurchaseInvoiceId);
        Assert.Equal(id, dn.PurchaseReturnId);
        Assert.Equal(Warehouse, dn.WarehouseId);
        Assert.True(dn.StockIssued);
        Assert.StartsWith("DN-", dn.DebitNoteNumber);
        Assert.NotNull(dn.AppliedFinanceBillId);

        var ap = await bills.GetByIdAsync(dn.AppliedFinanceBillId!.Value, Tenant);
        Assert.NotNull(ap);
        Assert.Equal(0m, ap!.Balance);
    }

    [Fact]
    public async Task Complete_WithoutPurchaseInvoice_Fails()
    {
        await using var db = Db();
        var (returns, _, _, _) = Services(db);
        var id = await returns.CreateAsync(new CreatePurchaseReturnDto
        {
            VendorId = Vendor,
            ReturnDate = DateTime.UtcNow.Date,
            Reason = "No PI",
            Lines =
            [
                new CreatePurchaseReturnLineDto { ItemId = Item, Quantity = 1, UnitPrice = 10m }
            ]
        }, Tenant, User);

        Assert.True(await returns.ApproveAsync(id, Tenant, User));
        Assert.False(await returns.CompleteAsync(id, Tenant, User));
        Assert.Equal("Approved", (await returns.GetByIdAsync(id, Tenant))!.Status);
        Assert.Null((await returns.GetByIdAsync(id, Tenant))!.DebitNoteId);
    }

    [Fact]
    public async Task Pending_Only_Editable_And_Cancel_Works()
    {
        await using var db = Db();
        var (returns, _, _, _) = Services(db);
        var id = await returns.CreateAsync(new CreatePurchaseReturnDto
        {
            VendorId = Vendor,
            ReturnDate = DateTime.UtcNow.Date,
            Reason = "Wrong item",
            TotalAmount = 10m
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal("Pending", (await returns.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await returns.ApproveAsync(id, Tenant, User));
        Assert.False(await returns.UpdateAsync(new UpdatePurchaseReturnDto { Id = id, Reason = "x" }, Tenant, User));
        Assert.True(await returns.CancelAsync(id, Tenant, User));
        Assert.Equal("Cancelled", (await returns.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task Complete_Fails_When_Stock_Insufficient()
    {
        await using var db = Db();
        db.StockBalances.Add(new StockBalance
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ItemId = Item,
            WarehouseId = Warehouse,
            CurrentQuantity = 1m,
            ReservedQuantity = 0m
        });
        await db.SaveChangesAsync();

        var (returns, pi, _, _) = Services(db);
        var piId = await CreateAndApprovePiAsync(pi, qty: 5, price: 10m);
        var id = await returns.CreateAsync(new CreatePurchaseReturnDto
        {
            VendorId = Vendor,
            PurchaseInvoiceId = piId,
            WarehouseId = Warehouse,
            Reason = "Short stock",
            Lines =
            [
                new CreatePurchaseReturnLineDto { ItemId = Item, Quantity = 5, UnitPrice = 10m }
            ]
        }, Tenant, User);

        Assert.True(await returns.ApproveAsync(id, Tenant, User));
        Assert.False(await returns.CompleteAsync(id, Tenant, User));
        Assert.Equal("Approved", (await returns.GetByIdAsync(id, Tenant))!.Status);
        Assert.Null((await returns.GetByIdAsync(id, Tenant))!.DebitNoteId);
    }
}
