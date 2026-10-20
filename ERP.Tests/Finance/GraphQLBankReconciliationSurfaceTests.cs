using ERP.Api.GraphQL.Finance;
using ERP.Application.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class GraphQLBankReconciliationSurfaceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
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
    public void Surface_ExposesReconciliationOps()
    {
        var queries = typeof(FinanceQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(FinanceMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetReconciliation", queries);
        Assert.Contains("ListReconciliations", queries);
        Assert.Contains("CreateReconciliation", mutations);
        Assert.Contains("SetTransactionReconciled", mutations);
        Assert.Contains("CloseReconciliation", mutations);
    }

    [Fact]
    public async Task GraphQL_Reconciliation_Open_Tick_List_Close()
    {
        await using var db = Db();
        var bank = BankSvc(db);
        var gql = Mutations(bank);
        var queries = Queries(bank);

        var bankId = await bank.CreateAsync(new CreateBankDto
        {
            TenantId = Tenant,
            BankName = "GQL Recon Bank",
            BankCode = "GRB",
            IsActive = true
        });
        var accountId = await bank.AddBankAccountAsync(new CreateBankAccountDto
        {
            TenantId = Tenant,
            BankId = bankId,
            AccountNumber = "ACC-GQL-R1",
            AccountName = "Ops",
            Currency = "USD",
            Balance = 100,
            GLAccountId = Guid.NewGuid()
        }, Tenant, User);

        var day = new DateTime(2026, 10, 1);
        var deposit = await bank.AddBankTransactionAsync(new CreateBankTransactionDto
        {
            BankAccountId = accountId,
            Amount = 50,
            CurrencyCode = "USD",
            TransactionDate = day
        }, Tenant, User);
        var withdrawal = await bank.AddBankTransactionAsync(new CreateBankTransactionDto
        {
            BankAccountId = accountId,
            Amount = -20,
            CurrencyCode = "USD",
            TransactionDate = day
        }, Tenant, User);

        var reconId = await gql.CreateReconciliation(new CreateBankReconciliationDto
        {
            BankAccountId = accountId,
            ReconciliationDate = day,
            OpeningBalance = 100,
            ClosingBalance = 130
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, reconId);

        Assert.False(await gql.CloseReconciliation(reconId, Tenant, User));

        Assert.True(await gql.SetTransactionReconciled(reconId, deposit, true, Tenant, User));
        Assert.True(await gql.SetTransactionReconciled(reconId, withdrawal, true, Tenant, User));

        var open = await queries.GetReconciliation(reconId, Tenant);
        Assert.NotNull(open);
        Assert.Equal(0m, open!.Difference);
        Assert.Equal(2, open.ReconciledTransactionCount);
        Assert.Equal("Open", open.Status);

        var listed = (await queries.ListReconciliations(accountId, Tenant)).ToList();
        Assert.Contains(listed, r => r.Id == reconId || r.ReconciliationId == reconId);

        Assert.True(await gql.CloseReconciliation(reconId, Tenant, User));
        var closed = await queries.GetReconciliation(reconId, Tenant);
        Assert.Equal("Closed", closed!.Status);
        Assert.False(await gql.SetTransactionReconciled(reconId, deposit, false, Tenant, User));
    }
}
