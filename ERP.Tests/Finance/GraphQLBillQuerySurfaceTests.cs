using ERP.Api.GraphQL.Finance;
using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class GraphQLBillQuerySurfaceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Vendor = Guid.NewGuid();
    private static readonly Guid Gl = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static InvoiceService Invoices(ERPDbContext db) =>
        new(new ApplicationInvoiceRepositoryAdapter(db));

    private static FinanceQueries Queries(InvoiceService invoices) => new(
        accountingService: null!,
        invoiceService: invoices,
        paymentService: null!,
        bankService: null!,
        fixedAssetService: null!);

    private static FinanceMutations Mutations(InvoiceService invoices) => new(
        accountingService: null!,
        invoiceService: invoices,
        billService: null!,
        paymentService: null!,
        bankService: null!,
        fixedAssetService: null!);

    [Fact]
    public void Surface_ExposesBillQueries()
    {
        var names = typeof(FinanceQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetBill", names);
        Assert.Contains("SearchBills", names);
        Assert.Contains("GetBillLines", names);
    }

    [Fact]
    public async Task GraphQL_Bill_Create_Get_Search_Lines()
    {
        await using var db = Db();
        db.ChartOfAccounts.Add(new ChartOfAccount
        {
            Id = Gl,
            AccountId = Gl,
            TenantId = Tenant,
            AccountCode = "5100",
            AccountName = "COGS",
            AccountType = "Expense",
            IsPostingAllowed = true,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var invoices = Invoices(db);
        var gql = Mutations(invoices);
        var queries = Queries(invoices);

        var billId = await gql.AddBill(new BillCreateDto
        {
            VendorId = Vendor,
            BillNumber = "AP-GQL-Q1",
            BillDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(20),
            Status = "Draft"
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, billId);

        var lineId = await gql.AddBillLine(new BillLineCreateDto
        {
            Description = "Widgets",
            Quantity = 2,
            UnitPrice = 25,
            TaxRate = 0,
            GLAccountId = Gl
        }, billId, Tenant, User);
        Assert.NotEqual(Guid.Empty, lineId);

        var bill = await queries.GetBill(billId, Tenant);
        Assert.NotNull(bill);
        Assert.Equal("AP-GQL-Q1", bill!.BillNumber);
        Assert.Equal(50m, bill.TotalAmount);

        var lines = (await queries.GetBillLines(billId, Tenant)).ToList();
        Assert.Single(lines);
        Assert.Equal(50m, lines[0].LineTotal);

        var page = await queries.SearchBills(Tenant, vendorId: Vendor, status: "Draft");
        Assert.Contains(page.Items, b => b.Id == billId || b.BillNumber == "AP-GQL-Q1");
        Assert.True(page.TotalCount >= 1);

        Assert.Null(await queries.GetBill(billId, Guid.NewGuid()));
    }
}
