using ERP.Api.GraphQL.Finance;
using ERP.Application.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class GraphQLBankMasterTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static BankService BankSvc(ERPDbContext db) =>
        new(new BankRepository(db), new BankAccountRepository(db), db);

    private static FinanceMutations Mutations(BankService bank) => new(
        accountingService: null!,
        invoiceService: null!,
        billService: null!,
        paymentService: null!,
        bankService: bank,
        fixedAssetService: null!);

    private static FinanceQueries Queries(BankService bank) => new(
        accountingService: null!,
        invoiceService: null!,
        paymentService: null!,
        bankService: bank,
        fixedAssetService: null!);

    [Fact]
    public void Surface_ExposesBankMaster()
    {
        var queries = typeof(FinanceQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(FinanceMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetBank", queries);
        Assert.Contains("GetBanks", queries);
        Assert.Contains("SearchBanks", queries);
        Assert.Contains("AddBank", mutations);
        Assert.Contains("UpdateBank", mutations);
        Assert.Contains("DeleteBank", mutations);
    }

    [Fact]
    public async Task GraphQL_Bank_Crud_TenantFailClosed()
    {
        await using var db = Db();
        var bank = BankSvc(db);
        var gql = Mutations(bank);
        var queries = Queries(bank);

        var id = await gql.AddBank(new CreateBankDto
        {
            BankName = "National Bank",
            BankCode = "NB01",
            CompanyId = Guid.NewGuid(),
            IsActive = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        Assert.Equal("National Bank", (await queries.GetBank(id, Tenant))!.BankName);
        Assert.Null(await queries.GetBank(id, OtherTenant));
        Assert.Null(await queries.GetBank(id, Guid.Empty));

        Assert.Contains(await queries.GetBanks(Tenant), b => b.Id == id);
        Assert.Empty(await queries.GetBanks(OtherTenant));
        Assert.Contains(await queries.SearchBanks("National", Tenant), b => b.Id == id);

        Assert.True(await gql.UpdateBank(new UpdateBankDto
        {
            Id = id,
            BankName = "National Bank Ltd",
            BankCode = "NB01",
            IsActive = true
        }, Tenant, User));
        Assert.Equal("National Bank Ltd", (await queries.GetBank(id, Tenant))!.BankName);
        Assert.False(await gql.UpdateBank(new UpdateBankDto
        {
            Id = id,
            BankName = "Hacked",
            BankCode = "NB01",
            IsActive = true
        }, OtherTenant, User));

        Assert.True(await gql.DeleteBank(id, Tenant));
        Assert.Null(await queries.GetBank(id, Tenant));
    }
}
