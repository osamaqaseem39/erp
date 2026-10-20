using ERP.Api.GraphQL.CRM;
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

public class GraphQLReturnOrderTests
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

    private static (ReturnOrderService Returns, SalesInvoiceService Sales) Services(ERPDbContext db)
    {
        var stock = new StockService(new StockBalanceRepository(db));
        var finance = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var sales = new SalesInvoiceService(
            new SalesInvoiceRepository(db),
            new SalesInvoiceLineRepository(db),
            finance,
            new TestCoreSettings());
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
        return (returns, sales);
    }

    [Fact]
    public void Surface_ExposesReturnOrderQueriesAndWorkflow()
    {
        var queries = typeof(CRMQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(CRMQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(CRMMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(CRMMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetReturnOrder", queries);
        Assert.Contains("GetReturnOrders", queries);

        Assert.Contains("CreateReturnOrder", mutations);
        Assert.Contains("UpdateReturnOrder", mutations);
        Assert.Contains("DeleteReturnOrder", mutations);
        Assert.Contains("ApproveReturnOrder", mutations);
        Assert.Contains("CompleteReturnOrder", mutations);
        Assert.Contains("CancelReturnOrder", mutations);
    }

    [Fact]
    public async Task GraphQL_Create_Approve_Complete_ReturnOrder()
    {
        await using var db = Db();
        db.Customers.Add(new ERP.Domain.CRM.Customer
        {
            Id = Customer,
            TenantId = Tenant,
            CustomerCode = "C-GQL-RT",
            CustomerName = "Acme",
            CompanyName = "Acme Ltd"
        });
        db.Warehouses.Add(new Warehouse
        {
            Id = Warehouse,
            TenantId = Tenant,
            WarehouseCode = "WH-GQL",
            WarehouseName = "Main"
        });
        await db.SaveChangesAsync();

        var (returns, sales) = Services(db);
        var queries = new CRMQueries();
        var mutations = new CRMMutations();

        var siId = await sales.CreateAsync(new CreateSalesInvoiceDto
        {
            InvoiceNumber = "SI-GQL-RT",
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
                    UnitPrice = 25,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);
        Assert.True(await sales.PostInvoiceAsync(siId, Tenant, User));

        var id = await mutations.CreateReturnOrder(returns, new CreateReturnOrderDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            ReturnNumber = "RT-GQL-1",
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

        var pending = await queries.GetReturnOrder(returns, id, Tenant);
        Assert.Equal("Pending", pending!.Status);

        var listed = (await queries.GetReturnOrders(returns, Tenant, status: "Pending")).ToList();
        Assert.Contains(listed, r => r.Id == id);

        Assert.False(await mutations.CompleteReturnOrder(returns, id, Tenant, User));
        Assert.True(await mutations.ApproveReturnOrder(returns, id, Tenant, User));
        Assert.True(await mutations.CompleteReturnOrder(returns, id, Tenant, User));

        var done = await queries.GetReturnOrder(returns, id, Tenant);
        Assert.Equal("Completed", done!.Status);
        Assert.NotNull(done.CreditNoteId);
    }

    [Fact]
    public async Task GraphQL_Cancel_Approved_ReturnOrder()
    {
        await using var db = Db();
        db.Customers.Add(new ERP.Domain.CRM.Customer
        {
            Id = Customer,
            TenantId = Tenant,
            CustomerCode = "C-GQL-CNL",
            CustomerName = "Beta",
            CompanyName = "Beta LLC"
        });
        await db.SaveChangesAsync();

        var (returns, _) = Services(db);
        var mutations = new CRMMutations();
        var queries = new CRMQueries();

        var id = await mutations.CreateReturnOrder(returns, new CreateReturnOrderDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            CustomerId = Customer,
            ReturnDate = DateTime.UtcNow.Date,
            Reason = "Wrong item",
            TotalAmount = 10m
        }, Tenant, User);
        Assert.True(await mutations.ApproveReturnOrder(returns, id, Tenant, User));
        Assert.True(await mutations.CancelReturnOrder(returns, id, Tenant, User));
        Assert.Equal("Cancelled", (await queries.GetReturnOrder(returns, id, Tenant))!.Status);
    }
}
