using ERP.Tests;
using ERP.Application.Core.Settings;
using ERP.Application.Finance;
using ERP.Application.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Sales;
using ERP.Shared.DTOs.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Sales;

public class SalesInvoiceWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static SalesInvoiceService Service(ERPDbContext db, TestCoreSettings? settings = null) =>
        new(new SalesInvoiceRepository(db), new SalesInvoiceLineRepository(db), new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db)), settings ?? new TestCoreSettings(), db);

    private static async Task<Guid> CreateDraft(SalesInvoiceService sales) =>
        await sales.CreateAsync(new CreateSalesInvoiceDto
        {
            InvoiceNumber = $"SI-WF-{Guid.NewGuid():N}"[..16],
            CustomerId = Customer,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            Status = "Posted", // ignored — create always Draft (ERP-FIX-5)
            Lines =
            [
                new CreateSalesInvoiceLineDto
                {
                    ItemId = Item,
                    Description = "Line",
                    Quantity = 1,
                    UnitPrice = 10,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);

    [Fact]
    public async Task Create_ForcesDraft_IgnoresClientStatus()
    {
        await using var db = Db();
        var sales = Service(db);
        var id = await CreateDraft(sales);
        Assert.Equal("Draft", (await sales.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task Update_AfterPost_Rejected()
    {
        await using var db = Db();
        var sales = Service(db);
        var id = await CreateDraft(sales);
        Assert.True(await sales.PostInvoiceAsync(id, Tenant, User));

        var ok = await sales.UpdateAsync(new UpdateSalesInvoiceDto
        {
            Id = id,
            InvoiceNumber = "X",
            CustomerId = Customer,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(1),
            Status = "Draft",
            Lines = []
        }, Tenant, User);

        Assert.False(ok);
        Assert.Equal("Posted", (await sales.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task Update_CannotFreeSetStatus_WhileDraft()
    {
        await using var db = Db();
        var sales = Service(db);
        var id = await CreateDraft(sales);

        Assert.True(await sales.UpdateAsync(new UpdateSalesInvoiceDto
        {
            Id = id,
            InvoiceNumber = "SI-KEEP",
            CustomerId = Customer,
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            Status = "Posted",
            Lines =
            [
                new UpdateSalesInvoiceLineDto
                {
                    ItemId = Item,
                    Description = "Line",
                    Quantity = 1,
                    UnitPrice = 10,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, User));

        Assert.Equal("Draft", (await sales.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task IllegalTransition_VoidFromUnknown_Rejected_PostThenVoid_Ok()
    {
        await using var db = Db();
        var sales = Service(db);
        var id = await CreateDraft(sales);

        Assert.True(await sales.PostInvoiceAsync(id, Tenant, User));
        Assert.False(await sales.PostInvoiceAsync(id, Tenant, User)); // illegal: Posted → Posted
        Assert.True(await sales.VoidInvoiceAsync(id, Tenant, User));
        Assert.Equal("Voided", (await sales.GetByIdAsync(id, Tenant))!.Status);
        Assert.False(await sales.VoidInvoiceAsync(id, Tenant, User)); // terminal
        Assert.False(await sales.DeleteAsync(id, Tenant));
    }

    [Fact]
    public async Task Unpost_Denied_WhenSettingOff()
    {
        await using var db = Db();
        var settings = new TestCoreSettings();
        var sales = Service(db, settings);
        var id = await CreateDraft(sales);
        Assert.True(await sales.PostInvoiceAsync(id, Tenant, User));
        Assert.False(await sales.UnpostInvoiceAsync(id, Tenant, User));
        Assert.Equal("Posted", (await sales.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task Unpost_WhenAllowed_ReturnsToDraft()
    {
        await using var db = Db();
        var settings = new TestCoreSettings();
        await settings.SetValueAsync(SettingsCatalog.AllowUnpostKey, "true");
        var sales = Service(db, settings);
        var id = await CreateDraft(sales);
        Assert.True(await sales.PostInvoiceAsync(id, Tenant, User));
        Assert.True(await sales.UnpostInvoiceAsync(id, Tenant, User));
        Assert.Equal("Draft", (await sales.GetByIdAsync(id, Tenant))!.Status);
    }
}
