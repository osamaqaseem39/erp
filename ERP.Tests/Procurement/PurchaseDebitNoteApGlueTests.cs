using ERP.Application.Finance;
using ERP.Application.Inventory;
using ERP.Application.Procurement;
using ERP.Domain.Finance;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Procurement;

public class PurchaseDebitNoteApGlueTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Creator = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();
    private static readonly Guid Item = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static (PurchaseInvoiceService Pi, PurchaseDebitNoteService Debit, BillService Bills, AccountingService Accounting) Services(
        ERPDbContext db, bool withGl = false, bool withStock = false)
    {
        var bills = new BillService(new BillRepository(db));
        var accounting = new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            new ApplicationChartOfAccountRepositoryAdapter(db),
            db);
        var stock = withStock ? new StockService(new StockBalanceRepository(db)) : null;
        var pi = ERP.Tests.TestHelpers.ProcurementTestServices.CreatePurchaseInvoiceService(db, bills);
        var debit = new PurchaseDebitNoteService(
            new PurchaseDebitNoteRepository(db),
            new PurchaseDebitNoteLineRepository(db),
            new PurchaseInvoiceRepository(db),
            bills,
            withGl ? accounting : null,
            withGl ? db : null,
            stock);
        return (pi, debit, bills, accounting);
    }

    private static async Task<Guid> CreateAndApprovePiAsync(PurchaseInvoiceService pi, decimal qty, decimal price, decimal taxRate)
    {
        var id = await pi.CreateAsync(new CreatePurchaseInvoiceDto
        {
            VendorId = Vendor,
            InvoiceNumber = "PI-DN-1",
            Details =
            [
                new CreatePurchaseInvoiceDetailDto
                {
                    ItemId = Item,
                    Quantity = qty,
                    UnitPrice = price,
                    TaxRate = taxRate
                }
            ]
        }, Tenant, Creator);
        Assert.True(await pi.ApproveAsync(id, Tenant, Approver));
        return id;
    }

    [Fact]
    public async Task Post_DebitNote_ReducesApBalance_And_IsIdempotent()
    {
        await using var db = Db();
        var (pi, debit, bills, _) = Services(db);

        var piId = await CreateAndApprovePiAsync(pi, qty: 2, price: 40, taxRate: 10);
        // PI total = 80 + 8 = 88

        var dnId = await debit.CreateAsync(new CreatePurchaseDebitNoteDto
        {
            DebitNoteNumber = "DN-1",
            DebitNoteDate = DateTime.UtcNow.Date,
            VendorId = Vendor,
            OriginalPurchaseInvoiceId = piId,
            Lines =
            [
                new CreatePurchaseDebitNoteLineDto
                {
                    ItemId = Item,
                    Description = "Return goods",
                    Quantity = 1,
                    UnitPrice = 40,
                    TaxRate = 10,
                    LineNumber = 1
                }
            ]
        }, Tenant, Creator);

        Assert.True(await debit.PostAsync(dnId, Tenant, Approver));
        Assert.True(await debit.PostAsync(dnId, Tenant, Approver)); // idempotent

        var dn = await debit.GetByIdAsync(dnId, Tenant);
        Assert.Equal("Posted", dn!.Status);
        Assert.Equal(44m, dn.TotalAmount); // 40 + 4 tax
        Assert.NotNull(dn.AppliedFinanceBillId);

        var ap = await bills.GetByIdAsync(dn.AppliedFinanceBillId!.Value, Tenant);
        Assert.NotNull(ap);
        Assert.Equal(44m, ap!.Balance); // 88 - 44
        Assert.Equal("PartiallyPaid", ap.Status);
        Assert.Equal("Approved", (await pi.GetByIdAsync(piId, Tenant))!.Status);
    }

    [Fact]
    public async Task Void_DebitNote_RestoresApBalance()
    {
        await using var db = Db();
        var (pi, debit, bills, _) = Services(db);

        var piId = await CreateAndApprovePiAsync(pi, qty: 2, price: 50, taxRate: 0);
        var dnId = await debit.CreateAsync(new CreatePurchaseDebitNoteDto
        {
            DebitNoteNumber = "DN-VOID",
            DebitNoteDate = DateTime.UtcNow.Date,
            VendorId = Vendor,
            OriginalPurchaseInvoiceId = piId,
            Lines =
            [
                new CreatePurchaseDebitNoteLineDto
                {
                    ItemId = Item,
                    Description = "Partial return",
                    Quantity = 1,
                    UnitPrice = 50,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, Creator);

        Assert.True(await debit.PostAsync(dnId, Tenant, Approver));
        Assert.Equal(50m, (await bills.GetByVendorAsync(Vendor, Tenant)).First().Balance);

        Assert.True(await debit.VoidAsync(dnId, new VoidPurchaseDebitNoteDto { Reason = "Mistake" }, Tenant, Approver));
        var dn = await debit.GetByIdAsync(dnId, Tenant);
        Assert.Equal("Voided", dn!.Status);
        Assert.Null(dn.AppliedFinanceBillId);

        var ap = (await bills.GetByVendorAsync(Vendor, Tenant)).First();
        Assert.Equal(100m, ap.Balance);
        Assert.Equal("Issued", ap.Status);
    }

    [Fact]
    public async Task Post_Fails_When_AmountExceedsApBalance()
    {
        await using var db = Db();
        var (pi, debit, _, _) = Services(db);

        var piId = await CreateAndApprovePiAsync(pi, qty: 1, price: 10, taxRate: 0);
        var dnId = await debit.CreateAsync(new CreatePurchaseDebitNoteDto
        {
            DebitNoteNumber = "DN-OVER",
            DebitNoteDate = DateTime.UtcNow.Date,
            VendorId = Vendor,
            OriginalPurchaseInvoiceId = piId,
            Lines =
            [
                new CreatePurchaseDebitNoteLineDto
                {
                    Description = "Too much",
                    Quantity = 1,
                    UnitPrice = 99,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, Creator);

        Assert.False(await debit.PostAsync(dnId, Tenant, Approver));
        Assert.Equal("Draft", (await debit.GetByIdAsync(dnId, Tenant))!.Status);
    }

    [Fact]
    public async Task Post_Fails_When_PiNotApproved()
    {
        await using var db = Db();
        var (pi, debit, _, _) = Services(db);

        var piId = await pi.CreateAsync(new CreatePurchaseInvoiceDto
        {
            VendorId = Vendor,
            InvoiceNumber = "PI-DRAFT",
            Details =
            [
                new CreatePurchaseInvoiceDetailDto { ItemId = Item, Quantity = 1, UnitPrice = 20, TaxRate = 0 }
            ]
        }, Tenant, Creator);

        var dnId = await debit.CreateAsync(new CreatePurchaseDebitNoteDto
        {
            DebitNoteNumber = "DN-DRAFT-PI",
            DebitNoteDate = DateTime.UtcNow.Date,
            VendorId = Vendor,
            OriginalPurchaseInvoiceId = piId,
            Lines =
            [
                new CreatePurchaseDebitNoteLineDto
                {
                    Description = "Return",
                    Quantity = 1,
                    UnitPrice = 20,
                    TaxRate = 0,
                    LineNumber = 1
                }
            ]
        }, Tenant, Creator);

        Assert.False(await debit.PostAsync(dnId, Tenant, Approver));
    }

    [Fact]
    public async Task Post_WithAccountDefaults_CreatesGlJournal_And_Void_Reverses()
    {
        await using var db = Db();
        var (pi, debit, _, accounting) = Services(db, withGl: true);

        var returnsId = await accounting.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "5100", AccountName = "Purchase Returns", AccountType = "Expense", IsPostingAllowed = true
        }, Tenant, Approver);
        var apCoaId = await accounting.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "2100", AccountName = "Accounts Payable", AccountType = "Liability", IsPostingAllowed = true
        }, Tenant, Approver);
        Assert.NotEqual(Guid.Empty, returnsId);
        Assert.NotEqual(Guid.Empty, apCoaId);

        db.AccountDefaults.AddRange(
            new AccountDefault
            {
                Id = Guid.NewGuid(), AccountDefaultId = Guid.NewGuid(), TenantId = Tenant,
                Purpose = AccountPurposes.PurchaseReturns, AccountId = returnsId, IsActive = true
            },
            new AccountDefault
            {
                Id = Guid.NewGuid(), AccountDefaultId = Guid.NewGuid(), TenantId = Tenant,
                Purpose = AccountPurposes.AccountsPayable, AccountId = apCoaId, IsActive = true
            });
        await db.SaveChangesAsync();

        var piId = await CreateAndApprovePiAsync(pi, qty: 1, price: 50, taxRate: 0);
        var dnId = await debit.CreateAsync(new CreatePurchaseDebitNoteDto
        {
            DebitNoteNumber = "DN-GL-1",
            DebitNoteDate = DateTime.UtcNow.Date,
            VendorId = Vendor,
            OriginalPurchaseInvoiceId = piId,
            Lines =
            [
                new CreatePurchaseDebitNoteLineDto
                {
                    ItemId = Item, Description = "Debit", Quantity = 1, UnitPrice = 20, TaxRate = 0, LineNumber = 1
                }
            ]
        }, Tenant, Creator);

        Assert.True(await debit.PostAsync(dnId, Tenant, Approver));
        var posted = await debit.GetByIdAsync(dnId, Tenant);
        Assert.NotNull(posted!.PostedJournalId);

        var jv = await accounting.GetJournalByIdAsync(posted.PostedJournalId!.Value, Tenant);
        Assert.NotNull(jv);
        Assert.Equal("Posted", jv!.Status);
        Assert.Equal(20m, jv.TotalDebit);
        Assert.Equal(20m, jv.TotalCredit);

        Assert.True(await debit.VoidAsync(dnId, new VoidPurchaseDebitNoteDto { Reason = "Undo" }, Tenant, Approver));
        var voided = await debit.GetByIdAsync(dnId, Tenant);
        Assert.Equal("Voided", voided!.Status);
        Assert.Null(voided.PostedJournalId);
        Assert.Equal("Reversed", (await accounting.GetJournalByIdAsync(jv.JournalId!.Value, Tenant))!.Status);
    }

    [Fact]
    public async Task Post_WithWarehouse_IssuesStock_And_Void_ReceivesBack()
    {
        await using var db = Db();
        db.Warehouses.Add(new Warehouse
        {
            Id = Warehouse, TenantId = Tenant, WarehouseCode = "WH1", WarehouseName = "Main"
        });
        await db.SaveChangesAsync();

        var (pi, debit, _, _) = Services(db, withStock: true);
        var stock = new StockService(new StockBalanceRepository(db));
        Assert.True(await stock.ReceiveStockAsync(Tenant, Warehouse, [(Item, 5m)], Approver.ToString()));

        var piId = await CreateAndApprovePiAsync(pi, qty: 1, price: 30, taxRate: 0);
        var dnId = await debit.CreateAsync(new CreatePurchaseDebitNoteDto
        {
            DebitNoteNumber = "DN-STK-1",
            DebitNoteDate = DateTime.UtcNow.Date,
            VendorId = Vendor,
            OriginalPurchaseInvoiceId = piId,
            WarehouseId = Warehouse,
            Lines =
            [
                new CreatePurchaseDebitNoteLineDto
                {
                    ItemId = Item, Description = "Return", Quantity = 3, UnitPrice = 10, TaxRate = 0, LineNumber = 1
                }
            ]
        }, Tenant, Creator);

        Assert.True(await debit.PostAsync(dnId, Tenant, Approver));
        var posted = await debit.GetByIdAsync(dnId, Tenant);
        Assert.True(posted!.StockIssued);

        var bal = await db.StockBalances.FirstOrDefaultAsync(s =>
            s.ItemId == Item && s.WarehouseId == Warehouse && s.TenantId == Tenant);
        Assert.NotNull(bal);
        Assert.Equal(2m, bal!.CurrentQuantity); // 5 - 3

        Assert.True(await debit.VoidAsync(dnId, new VoidPurchaseDebitNoteDto { Reason = "Undo stock" }, Tenant, Approver));
        bal = await db.StockBalances.FirstAsync(s =>
            s.ItemId == Item && s.WarehouseId == Warehouse && s.TenantId == Tenant);
        Assert.Equal(5m, bal.CurrentQuantity);
        Assert.False((await debit.GetByIdAsync(dnId, Tenant))!.StockIssued);
    }
}
