using ERP.Application.Finance;
using ERP.Application.Inventory;
using ERP.Application.Sales;
using ERP.Domain.Finance;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Sales;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Sales;

public class SalesCreditNoteArGlueTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static (SalesInvoiceService Sales, SalesCreditNoteService Credit, InvoiceService Finance, AccountingService Accounting) Services(
        ERPDbContext db, bool withGl = false, bool withStock = false)
    {
        var finance = new InvoiceService(new ApplicationInvoiceRepositoryAdapter(db));
        var accounting = new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db);
        var stock = withStock ? new StockService(new StockBalanceRepository(db)) : null;
        var sales = new SalesInvoiceService(
            new SalesInvoiceRepository(db),
            new SalesInvoiceLineRepository(db),
            finance);
        var credit = new SalesCreditNoteService(
            new SalesCreditNoteRepository(db),
            new SalesCreditNoteLineRepository(db),
            new SalesInvoiceRepository(db),
            finance,
            withGl ? accounting : null,
            withGl ? db : null,
            stock);
        return (sales, credit, finance, accounting);
    }

    private static async Task<Guid> CreateAndPostSiAsync(SalesInvoiceService sales, string number, decimal qty, decimal price, decimal taxRate)
    {
        var id = await sales.CreateAsync(new CreateSalesInvoiceDto
        {
            InvoiceNumber = number,
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
                    TaxRate = taxRate,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);
        Assert.True(await sales.PostInvoiceAsync(id, Tenant, User));
        return id;
    }

    [Fact]
    public async Task Post_CreditNote_ReducesArBalance_And_IsIdempotent()
    {
        await using var db = Db();
        var (sales, credit, finance, _) = Services(db);

        var siId = await CreateAndPostSiAsync(sales, "SI-CN-1", qty: 2, price: 40, taxRate: 10);
        // SI total = 80 + 8 = 88

        var cnId = await credit.CreateAsync(new CreateSalesCreditNoteDto
        {
            CreditNoteNumber = "CN-1",
            CreditNoteDate = DateTime.UtcNow.Date,
            CustomerId = Customer,
            OriginalSalesInvoiceId = siId,
            Lines =
            [
                new CreateSalesCreditNoteLineDto
                {
                    ItemId = Item,
                    Description = "Return widget",
                    Quantity = 1,
                    UnitPrice = 40,
                    TaxRate = 10,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);

        Assert.True(await credit.PostAsync(cnId, Tenant, User));
        Assert.True(await credit.PostAsync(cnId, Tenant, User)); // idempotent

        var cn = await credit.GetByIdAsync(cnId, Tenant);
        Assert.Equal("Posted", cn!.Status);
        Assert.Equal(44m, cn.TotalAmount); // 40 + 4 tax
        Assert.NotNull(cn.AppliedFinanceInvoiceId);

        var ar = await finance.GetInvoiceByIdAsync(cn.AppliedFinanceInvoiceId!.Value, Tenant);
        Assert.NotNull(ar);
        Assert.Equal(44m, ar!.Balance); // 88 - 44
        Assert.Equal("PartiallyPaid", ar.Status);
        Assert.Equal("Posted", (await sales.GetByIdAsync(siId, Tenant))!.Status);
    }

    [Fact]
    public async Task Post_Rejected_When_CreditExceedsArBalance()
    {
        await using var db = Db();
        var (sales, credit, _, _) = Services(db);

        var siId = await CreateAndPostSiAsync(sales, "SI-CN-2", qty: 1, price: 10, taxRate: 0);
        // AR balance = 10

        var cnId = await credit.CreateAsync(new CreateSalesCreditNoteDto
        {
            CreditNoteNumber = "CN-OVER",
            CreditNoteDate = DateTime.UtcNow.Date,
            CustomerId = Customer,
            OriginalSalesInvoiceId = siId,
            Lines =
            [
                new CreateSalesCreditNoteLineDto
                {
                    ItemId = Item,
                    Description = "Too much",
                    Quantity = 2,
                    UnitPrice = 10,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);

        Assert.Equal(20m, (await credit.GetByIdAsync(cnId, Tenant))!.TotalAmount);
        Assert.False(await credit.PostAsync(cnId, Tenant, User));
        Assert.Equal("Draft", (await credit.GetByIdAsync(cnId, Tenant))!.Status);
    }

    [Fact]
    public async Task Void_RestoresArBalance()
    {
        await using var db = Db();
        var (sales, credit, finance, _) = Services(db);

        var siId = await CreateAndPostSiAsync(sales, "SI-CN-3", qty: 1, price: 100, taxRate: 0);

        var cnId = await credit.CreateAsync(new CreateSalesCreditNoteDto
        {
            CreditNoteNumber = "CN-VOID",
            CreditNoteDate = DateTime.UtcNow.Date,
            CustomerId = Customer,
            OriginalSalesInvoiceId = siId,
            Lines =
            [
                new CreateSalesCreditNoteLineDto
                {
                    ItemId = Item,
                    Description = "Credit",
                    Quantity = 1,
                    UnitPrice = 40,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, User);

        Assert.True(await credit.PostAsync(cnId, Tenant, User));
        var posted = await credit.GetByIdAsync(cnId, Tenant);
        var arId = posted!.AppliedFinanceInvoiceId!.Value;
        Assert.Equal(60m, (await finance.GetInvoiceByIdAsync(arId, Tenant))!.Balance);

        Assert.True(await credit.VoidAsync(cnId, new VoidSalesCreditNoteDto { Reason = "Mistake" }, Tenant, User));
        var voided = await credit.GetByIdAsync(cnId, Tenant);
        Assert.Equal("Voided", voided!.Status);
        Assert.Null(voided.AppliedFinanceInvoiceId);

        var ar = await finance.GetInvoiceByIdAsync(arId, Tenant);
        Assert.Equal(100m, ar!.Balance);
        Assert.Equal("Issued", ar.Status);
        Assert.Equal("Posted", (await sales.GetByIdAsync(siId, Tenant))!.Status);
    }

    [Fact]
    public async Task Post_WithAccountDefaults_CreatesGlJournal_And_Void_Reverses()
    {
        await using var db = Db();
        var (sales, credit, _, accounting) = Services(db, withGl: true);

        var returnsId = await accounting.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "4100", AccountName = "Sales Returns", AccountType = "Revenue", IsPostingAllowed = true
        }, Tenant, User);
        var arCoaId = await accounting.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "1200", AccountName = "Accounts Receivable", AccountType = "Asset", IsPostingAllowed = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, returnsId);
        Assert.NotEqual(Guid.Empty, arCoaId);

        db.AccountDefaults.AddRange(
            new AccountDefault
            {
                Id = Guid.NewGuid(), AccountDefaultId = Guid.NewGuid(), TenantId = Tenant,
                Purpose = AccountPurposes.SalesReturns, AccountId = returnsId, IsActive = true
            },
            new AccountDefault
            {
                Id = Guid.NewGuid(), AccountDefaultId = Guid.NewGuid(), TenantId = Tenant,
                Purpose = AccountPurposes.AccountsReceivable, AccountId = arCoaId, IsActive = true
            });
        await db.SaveChangesAsync();

        var siId = await CreateAndPostSiAsync(sales, "SI-CN-GL", qty: 1, price: 50, taxRate: 0);
        var cnId = await credit.CreateAsync(new CreateSalesCreditNoteDto
        {
            CreditNoteNumber = "CN-GL-1",
            CreditNoteDate = DateTime.UtcNow.Date,
            CustomerId = Customer,
            OriginalSalesInvoiceId = siId,
            Lines =
            [
                new CreateSalesCreditNoteLineDto
                {
                    ItemId = Item, Description = "Credit", Quantity = 1, UnitPrice = 20, TaxRate = 0, LineNumber = 1
                }
            ]
        }, Tenant, User);

        Assert.True(await credit.PostAsync(cnId, Tenant, User));
        var posted = await credit.GetByIdAsync(cnId, Tenant);
        Assert.NotNull(posted!.PostedJournalId);

        var jv = await accounting.GetJournalByIdAsync(posted.PostedJournalId!.Value, Tenant);
        Assert.NotNull(jv);
        Assert.Equal("Posted", jv!.Status);
        Assert.Equal(20m, jv.TotalDebit);
        Assert.Equal(20m, jv.TotalCredit);

        Assert.True(await credit.VoidAsync(cnId, new VoidSalesCreditNoteDto { Reason = "Undo" }, Tenant, User));
        var voided = await credit.GetByIdAsync(cnId, Tenant);
        Assert.Equal("Voided", voided!.Status);
        Assert.Null(voided.PostedJournalId);
        Assert.Equal("Reversed", (await accounting.GetJournalByIdAsync(jv.JournalId!.Value, Tenant))!.Status);
    }

    [Fact]
    public async Task Post_WithWarehouse_ReceivesStock_And_Void_IssuesBack()
    {
        await using var db = Db();
        db.Warehouses.Add(new Warehouse
        {
            Id = Warehouse, TenantId = Tenant, WarehouseCode = "WH1", WarehouseName = "Main"
        });
        await db.SaveChangesAsync();

        var (sales, credit, _, _) = Services(db, withStock: true);
        var siId = await CreateAndPostSiAsync(sales, "SI-CN-STK", qty: 1, price: 30, taxRate: 0);

        var cnId = await credit.CreateAsync(new CreateSalesCreditNoteDto
        {
            CreditNoteNumber = "CN-STK-1",
            CreditNoteDate = DateTime.UtcNow.Date,
            CustomerId = Customer,
            OriginalSalesInvoiceId = siId,
            WarehouseId = Warehouse,
            Lines =
            [
                new CreateSalesCreditNoteLineDto
                {
                    ItemId = Item, Description = "Return", Quantity = 3, UnitPrice = 10, TaxRate = 0, LineNumber = 1
                }
            ]
        }, Tenant, User);

        Assert.True(await credit.PostAsync(cnId, Tenant, User));
        var posted = await credit.GetByIdAsync(cnId, Tenant);
        Assert.True(posted!.StockReceived);

        var bal = await db.StockBalances.FirstOrDefaultAsync(s =>
            s.ItemId == Item && s.WarehouseId == Warehouse && s.TenantId == Tenant);
        Assert.NotNull(bal);
        Assert.Equal(3m, bal!.CurrentQuantity);

        Assert.True(await credit.VoidAsync(cnId, new VoidSalesCreditNoteDto { Reason = "Undo stock" }, Tenant, User));
        bal = await db.StockBalances.FirstAsync(s =>
            s.ItemId == Item && s.WarehouseId == Warehouse && s.TenantId == Tenant);
        Assert.Equal(0m, bal.CurrentQuantity);
        Assert.False((await credit.GetByIdAsync(cnId, Tenant))!.StockReceived);
    }
}
