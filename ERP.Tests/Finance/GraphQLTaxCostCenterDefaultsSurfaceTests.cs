using ERP.Api.GraphQL.Finance;
using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class GraphQLTaxCostCenterDefaultsSurfaceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Surface_ExposesTaxCostCenterAndDefaults()
    {
        var queries = typeof(FinanceQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(FinanceMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetTaxCode", queries);
        Assert.Contains("GetTaxCodes", queries);
        Assert.Contains("GetCostCenter", queries);
        Assert.Contains("GetCostCenters", queries);
        Assert.Contains("GetAccountDefaults", queries);
        Assert.Contains("GetAccountDefaultByPurpose", queries);
        Assert.Contains("GetVoucherDefaults", queries);

        Assert.Contains("AddTaxCode", mutations);
        Assert.Contains("UpdateTaxCode", mutations);
        Assert.Contains("DeleteTaxCode", mutations);
        Assert.Contains("AddCostCenter", mutations);
        Assert.Contains("UpdateCostCenter", mutations);
        Assert.Contains("DeleteCostCenter", mutations);
        Assert.Contains("SoftDeactivateCostCenter", mutations);
        Assert.Contains("UpsertAccountDefaults", mutations);
    }

    [Fact]
    public async Task GraphQL_CostCenter_Create_And_List()
    {
        await using var db = Db();
        var ccSvc = new CostCenterService(new CostCenterRepository(db));
        var gql = new FinanceMutations(
            accountingService: null!,
            invoiceService: null!,
            billService: null!,
            paymentService: null!,
            bankService: null!,
            fixedAssetService: null!);

        var id = await gql.AddCostCenter(ccSvc, new CreateCostCenterDto
        {
            CostCenterCode = "CC-GQL",
            CostCenterName = "GraphQL Ops"
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var queries = new FinanceQueries(
            accountingService: null!,
            invoiceService: null!,
            paymentService: null!,
            bankService: null!,
            fixedAssetService: null!);
        var listed = (await queries.GetCostCenters(ccSvc, Tenant, isActive: true)).ToList();
        Assert.Contains(listed, c => c.CostCenterId == id || c.Id == id);
        Assert.NotNull(await queries.GetCostCenter(ccSvc, id, Tenant));
    }

    [Fact]
    public async Task GraphQL_TaxCode_Create_Requires_Posting_Account()
    {
        await using var db = Db();
        var gl = Guid.NewGuid();
        db.ChartOfAccounts.Add(new ChartOfAccount
        {
            Id = gl, AccountId = gl, TenantId = Tenant,
            AccountCode = "2100", AccountName = "Output Tax", AccountType = "Liability",
            IsPostingAllowed = true, IsActive = true
        });
        await db.SaveChangesAsync();

        var accounts = new ApplicationChartOfAccountRepositoryAdapter(db);
        var taxSvc = new TaxCodeService(new TaxCodeRepository(db), accounts);
        var gql = new FinanceMutations(
            accountingService: null!,
            invoiceService: null!,
            billService: null!,
            paymentService: null!,
            bankService: null!,
            fixedAssetService: null!);

        var id = await gql.AddTaxCode(taxSvc, new CreateTaxCodeDto
        {
            TaxCode = "GST15",
            TaxName = "GST 15%",
            TaxType = "Sales",
            TaxRate = 15m,
            GLAccountId = gl
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var queries = new FinanceQueries(
            accountingService: null!,
            invoiceService: null!,
            paymentService: null!,
            bankService: null!,
            fixedAssetService: null!);
        var dto = await queries.GetTaxCode(taxSvc, id, Tenant);
        Assert.NotNull(dto);
        Assert.Equal("GST15", dto!.TaxCode);
        Assert.Equal(15m, dto.TaxRate);
    }
}
