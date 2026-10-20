using ERP.Application.Finance;
using ERP.Application.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Sales;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Sales;

public class Wave2SalesInvoiceArGlueTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Post_CreatesFinanceArInvoice()
    {
        await using var db = Db();
        var finance = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var sales = new SalesInvoiceService(
            new SalesInvoiceRepository(db),
            new SalesInvoiceLineRepository(db),
            finance,
            new TestCoreSettings());

        var id = await sales.CreateAsync(new CreateSalesInvoiceDto
        {
            InvoiceNumber = "SI-AR-1",
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
                    Quantity = 2,
                    UnitPrice = 40,
                    TaxRate = 10,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);

        Assert.True(await sales.PostInvoiceAsync(id, Tenant, User));
        Assert.Equal("Posted", (await sales.GetByIdAsync(id, Tenant))!.Status);

        var (ars, count) = await finance.SearchInvoicesAsync(new InvoiceQuery
        {
            TenantId = Tenant,
            CustomerId = Customer,
            PageNumber = 1,
            PageSize = 10
        });
        Assert.Equal(1, count);
        var ar = Assert.Single(ars);
        Assert.Equal("SI-AR-1", ar.InvoiceNumber);
        Assert.Equal(88m, ar.TotalAmount); // 80 + 8 tax
        Assert.Equal(88m, ar.Balance);
        Assert.Equal("Issued", ar.Status);

        Assert.False(await sales.PostInvoiceAsync(id, Tenant, User));
    }

    [Fact]
    public async Task Void_ReversesFinanceArInvoice()
    {
        await using var db = Db();
        var finance = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var sales = new SalesInvoiceService(
            new SalesInvoiceRepository(db),
            new SalesInvoiceLineRepository(db),
            finance,
            new TestCoreSettings());

        var id = await sales.CreateAsync(new CreateSalesInvoiceDto
        {
            InvoiceNumber = "SI-AR-VOID",
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
                    Quantity = 1,
                    UnitPrice = 50,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);

        Assert.True(await sales.PostInvoiceAsync(id, Tenant, User));
        Assert.True(await sales.VoidInvoiceAsync(id, Tenant, User));
        Assert.Equal("Voided", (await sales.GetByIdAsync(id, Tenant))!.Status);

        var (ars, _) = await finance.SearchInvoicesAsync(new InvoiceQuery
        {
            TenantId = Tenant,
            CustomerId = Customer,
            PageNumber = 1,
            PageSize = 10
        });
        var ar = Assert.Single(ars);
        Assert.Equal("Cancelled", ar.Status);
        Assert.Equal(0m, ar.Balance);
    }

    [Fact]
    public async Task Void_Fails_When_Ar_Has_Payments()
    {
        await using var db = Db();
        var finance = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var sales = new SalesInvoiceService(
            new SalesInvoiceRepository(db),
            new SalesInvoiceLineRepository(db),
            finance,
            new TestCoreSettings());

        var id = await sales.CreateAsync(new CreateSalesInvoiceDto
        {
            InvoiceNumber = "SI-AR-PAID",
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
                    Quantity = 1,
                    UnitPrice = 100,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);

        Assert.True(await sales.PostInvoiceAsync(id, Tenant, User));
        var (ars, _) = await finance.SearchInvoicesAsync(new InvoiceQuery
        {
            TenantId = Tenant,
            CustomerId = Customer,
            PageNumber = 1,
            PageSize = 10
        });
        var arId = ars.Single().InvoiceId != Guid.Empty ? ars.Single().InvoiceId : ars.Single().Id;
        Assert.True(await finance.ApplyPaymentToInvoiceAsync(new PaymentApplicationDto
        {
            InvoiceId = arId,
            Amount = 40
        }, Tenant, User));

        Assert.False(await sales.VoidInvoiceAsync(id, Tenant, User));
        Assert.Equal("Posted", (await sales.GetByIdAsync(id, Tenant))!.Status);
    }
}
