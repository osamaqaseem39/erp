using ERP.Application.CRM;
using ERP.Application.Finance;
using ERP.Application.Inventory;
using ERP.Application.Sales;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Infrastructure.Repositories.Sales;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.CRM;
using ERP.Shared.DTOs.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.CRM;

public class SalesReturnCreditNoteTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private sealed class FixedTenant : ITenantProvider
    {
        public Guid? GetTenantId() => Tenant;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static (ReturnOrderService Returns, SalesInvoiceService Sales, SalesCreditNoteService Credit) Services(ERPDbContext db)
    {
        var stock = new StockService(new StockBalanceRepository(db));
        var finance = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var sales = new SalesInvoiceService(
            new SalesInvoiceRepository(db),
            new SalesInvoiceLineRepository(db),
            finance);
        var credit = new SalesCreditNoteService(
            new SalesCreditNoteRepository(db),
            new SalesCreditNoteLineRepository(db),
            new SalesInvoiceRepository(db),
            finance,
            accounting: null,
            db: null,
            stock);
        var returns = new ReturnOrderService(
            new ReturnOrderRepository(db, new FixedTenant()),
            stock,
            credit);
        return (returns, sales, credit);
    }

    private static async Task<Guid> CreateAndPostSiAsync(SalesInvoiceService sales, decimal qty, decimal price)
    {
        var id = await sales.CreateAsync(new CreateSalesInvoiceDto
        {
            InvoiceNumber = "SI-RT-1",
            CustomerId = Customer,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            Status = "Draft",
            Lines =
            [
                new CreateSalesInvoiceLineDto
                {
                    ItemId = Item,
                    Description = "Widget",
                    Quantity = qty,
                    UnitPrice = price,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);
        Assert.True(await sales.PostInvoiceAsync(id, Tenant, User));
        return id;
    }

    [Fact]
    public async Task Approve_Complete_PostsSalesCreditNote_And_ReceivesStock()
    {
        await using var db = Db();
        db.Customers.Add(new ERP.Domain.CRM.Customer
        {
            Id = Customer,
            TenantId = Tenant,
            CustomerCode = "C1",
            CustomerName = "Acme",
            CompanyName = "Acme Ltd"
        });
        db.Warehouses.Add(new Warehouse
        {
            Id = Warehouse,
            TenantId = Tenant,
            WarehouseCode = "WH1",
            WarehouseName = "Main"
        });
        await db.SaveChangesAsync();

        var (returns, sales, credit) = Services(db);
        var siId = await CreateAndPostSiAsync(sales, qty: 2, price: 25m);

        var id = await returns.CreateAsync(new CreateReturnOrderDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            ReturnNumber = "RT-100",
            ReturnDate = DateTime.UtcNow.Date,
            CustomerId = Customer,
            SalesInvoiceId = siId,
            WarehouseId = Warehouse,
            Reason = "Damaged",
            Lines =
            [
                new CreateReturnOrderLineDto { ItemId = Item, Quantity = 2, UnitPrice = 25m }
            ]
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var created = await returns.GetByIdAsync(id, Tenant);
        Assert.Equal("Pending", created!.Status);
        Assert.Equal(50m, created.TotalAmount);
        Assert.Single(created.Lines);

        Assert.False(await returns.CompleteAsync(id, Tenant, User)); // must approve first
        Assert.True(await returns.ApproveAsync(id, Tenant, User));
        Assert.True(await returns.CompleteAsync(id, Tenant, User));

        var done = await returns.GetByIdAsync(id, Tenant);
        Assert.Equal("Completed", done!.Status);
        Assert.NotNull(done.CreditNoteId);

        var stockRow = await db.StockBalances.FirstOrDefaultAsync(s =>
            s.ItemId == Item && s.WarehouseId == Warehouse);
        Assert.NotNull(stockRow);
        Assert.Equal(2m, stockRow!.CurrentQuantity);

        var cn = await credit.GetByIdAsync(done.CreditNoteId!.Value, Tenant);
        Assert.NotNull(cn);
        Assert.Equal("Posted", cn!.Status);
        Assert.Equal(50m, cn.TotalAmount);
        Assert.Equal(siId, cn.OriginalSalesInvoiceId);
        Assert.Equal(id, cn.ReturnOrderId);
        Assert.True(cn.StockReceived);
        Assert.StartsWith("CN-", cn.CreditNoteNumber);
        Assert.NotNull(cn.AppliedFinanceInvoiceId);
    }

    [Fact]
    public async Task Complete_WithoutSalesInvoice_Fails()
    {
        await using var db = Db();
        db.Customers.Add(new ERP.Domain.CRM.Customer
        {
            Id = Customer,
            TenantId = Tenant,
            CustomerCode = "C0",
            CustomerName = "NoSi",
            CompanyName = "NoSi"
        });
        await db.SaveChangesAsync();

        var (returns, _, _) = Services(db);
        var id = await returns.CreateAsync(new CreateReturnOrderDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            CustomerId = Customer,
            ReturnDate = DateTime.UtcNow.Date,
            Reason = "No invoice",
            TotalAmount = 10m
        }, Tenant, User);
        Assert.True(await returns.ApproveAsync(id, Tenant, User));
        Assert.False(await returns.CompleteAsync(id, Tenant, User));
        Assert.Equal("Approved", (await returns.GetByIdAsync(id, Tenant))!.Status);
        Assert.Null((await returns.GetByIdAsync(id, Tenant))!.CreditNoteId);
    }

    [Fact]
    public async Task Pending_Only_Editable_And_Cancel_Works()
    {
        await using var db = Db();
        db.Customers.Add(new ERP.Domain.CRM.Customer
        {
            Id = Customer,
            TenantId = Tenant,
            CustomerCode = "C2",
            CustomerName = "Beta",
            CompanyName = "Beta LLC"
        });
        await db.SaveChangesAsync();

        var (returns, _, _) = Services(db);
        var id = await returns.CreateAsync(new CreateReturnOrderDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            CustomerId = Customer,
            ReturnDate = DateTime.UtcNow.Date,
            Reason = "Wrong item",
            TotalAmount = 10m
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal("Pending", (await returns.GetByIdAsync(id, Tenant))!.Status);

        Assert.True(await returns.ApproveAsync(id, Tenant, User));
        Assert.False(await returns.UpdateAsync(new UpdateReturnOrderDto { Id = id, Reason = "x" }, Tenant, User));
        Assert.True(await returns.CancelAsync(id, Tenant, User)); // Approved → Cancelled
        Assert.Equal("Cancelled", (await returns.GetByIdAsync(id, Tenant))!.Status);
    }
}
