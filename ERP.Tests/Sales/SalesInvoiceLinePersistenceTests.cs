using ERP.Tests;
using ERP.Application.Finance;
using ERP.Application.Sales;
using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Sales;
using ERP.Shared.DTOs.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Sales;

public class SalesInvoiceLinePersistenceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

    private static ERPDbContext Db(string name) => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(name)
        .Options);

    private static SalesInvoiceService Svc(ERPDbContext db) =>
        new(new SalesInvoiceRepository(db), new SalesInvoiceLineRepository(db), new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db)), new TestCoreSettings());

    [Fact]
    public async Task Create_PersistsLines_AndRecalculatesTotals()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = Db(dbName);
        var service = Svc(db);

        var id = await service.CreateAsync(new CreateSalesInvoiceDto
        {
            InvoiceNumber = "INV-1",
            CustomerId = Customer,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            Status = "Draft",
            SubTotal = 0,
            TaxAmount = 0,
            TotalAmount = 0,
            Lines =
            [
                new CreateSalesInvoiceLineDto
                {
                    ItemId = Item,
                    Description = "Widget",
                    Quantity = 2,
                    UnitPrice = 50,
                    TaxRate = 10,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);

        var loaded = await service.GetByIdAsync(id, Tenant);
        Assert.NotNull(loaded);
        Assert.Single(loaded!.Lines);
        Assert.Equal(Item, loaded.Lines[0].ItemId);
        Assert.Equal(100m, loaded.SubTotal);
        Assert.Equal(10m, loaded.TaxAmount);
        Assert.Equal(110m, loaded.TotalAmount);
        Assert.Equal(110m, loaded.BalanceAmount);
        Assert.Equal(100m, loaded.Lines[0].LineTotal - loaded.Lines[0].TaxAmount);
    }

    [Fact]
    public async Task Update_ReplacesLines()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = Db(dbName);
        var service = Svc(db);

        var id = await service.CreateAsync(new CreateSalesInvoiceDto
        {
            InvoiceNumber = "INV-2",
            CustomerId = Customer,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(14),
            Lines =
            [
                new CreateSalesInvoiceLineDto { ItemId = Item, Description = "A", Quantity = 1, UnitPrice = 10, TaxRate = 0, LineNumber = 1 }
            ]
        }, Tenant, User);

        var ok = await service.UpdateAsync(new UpdateSalesInvoiceDto
        {
            Id = id,
            InvoiceNumber = "INV-2",
            CustomerId = Customer,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(14),
            Status = "Draft",
            Lines =
            [
                new UpdateSalesInvoiceLineDto { ItemId = Item, Description = "B", Quantity = 3, UnitPrice = 20, TaxRate = 0, LineNumber = 1 }
            ]
        }, Tenant, User);

        Assert.True(ok);
        var loaded = await service.GetByIdAsync(id, Tenant);
        Assert.NotNull(loaded);
        Assert.Single(loaded!.Lines);
        Assert.Equal("B", loaded.Lines[0].Description);
        Assert.Equal(60m, loaded.TotalAmount);
    }

    [Fact]
    public async Task Update_PostedInvoice_IsRejected()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = Db(dbName);
        db.SalesInvoices.Add(new SalesInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            InvoiceNumber = "INV-P",
            CustomerId = Customer,
            Status = "Posted",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date
        });
        await db.SaveChangesAsync();

        var invoiceId = db.SalesInvoices.Single().Id;
        var service = Svc(db);

        var ok = await service.UpdateAsync(new UpdateSalesInvoiceDto
        {
            Id = invoiceId,
            InvoiceNumber = "INV-P",
            CustomerId = Customer,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date,
            Status = "Draft",
            Lines = []
        }, Tenant, User);

        Assert.False(ok);
    }
}
