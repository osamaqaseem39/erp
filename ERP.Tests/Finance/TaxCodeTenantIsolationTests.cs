using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class TaxCodeTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static (TaxCodeService Tax, AccountingService Accounting) Services(ERPDbContext db)
    {
        var accounts = new ApplicationChartOfAccountRepositoryAdapter(db);
        var accounting = new AccountingService(
            new ApplicationJournalRepositoryAdapter(db),
            accounts,
            db);
        var tax = new TaxCodeService(new TaxCodeRepository(db), accounts);
        return (tax, accounting);
    }

    private static async Task<Guid> AddPostingAccountAsync(
        AccountingService accounting, Guid tenantId, string code, string name)
    {
        var id = await accounting.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = code,
            AccountName = name,
            AccountType = "Liability",
            IsPostingAllowed = true
        }, tenantId, User);
        Assert.NotEqual(Guid.Empty, id);
        return id;
    }

    [Fact]
    public async Task Create_Get_List_FailClosed_And_Isolate()
    {
        await using var db = Db();
        var (tax, accounting) = Services(db);
        var glId = await AddPostingAccountAsync(accounting, TenantA, "2200", "Output Tax");

        var id = await tax.CreateAsync(new CreateTaxCodeDto
        {
            TaxCode = "VAT15",
            TaxName = "VAT 15%",
            TaxType = "VAT",
            TaxRate = 15m,
            GLAccountId = glId,
            IsActive = true
        }, TenantA, User);
        Assert.NotEqual(Guid.Empty, id);

        Assert.Equal(Guid.Empty, await tax.CreateAsync(new CreateTaxCodeDto
        {
            TaxCode = "X",
            TaxName = "X",
            TaxType = "VAT",
            TaxRate = 1m,
            GLAccountId = glId
        }, Guid.Empty, User));

        Assert.NotNull(await tax.GetByIdAsync(id, TenantA));
        Assert.Null(await tax.GetByIdAsync(id, TenantB));
        Assert.Null(await tax.GetByIdAsync(id, Guid.Empty));

        Assert.Single(await tax.GetAllAsync(TenantA));
        Assert.Empty(await tax.GetAllAsync(TenantB));
        Assert.Empty(await tax.GetAllAsync(Guid.Empty));
        Assert.Empty(await tax.GetActiveAsync(Guid.Empty));
    }

    [Fact]
    public async Task Update_Delete_Reject_CrossTenant_And_EmptyTenant()
    {
        await using var db = Db();
        var (tax, accounting) = Services(db);
        var glId = await AddPostingAccountAsync(accounting, TenantA, "2210", "GST");

        var id = await tax.CreateAsync(new CreateTaxCodeDto
        {
            TaxCode = "GST5",
            TaxName = "GST 5%",
            TaxType = "GST",
            TaxRate = 5m,
            GLAccountId = glId,
            IsActive = true
        }, TenantA, User);
        Assert.NotEqual(Guid.Empty, id);

        Assert.False(await tax.UpdateAsync(new UpdateTaxCodeDto
        {
            Id = id,
            TaxName = "Hacked"
        }, TenantB, User));
        Assert.False(await tax.UpdateAsync(new UpdateTaxCodeDto
        {
            Id = id,
            TaxName = "Hacked"
        }, Guid.Empty, User));

        Assert.False(await tax.DeleteAsync(id, TenantB));
        Assert.False(await tax.DeleteAsync(id, Guid.Empty));
        Assert.True(await tax.DeleteAsync(id, TenantA));
        Assert.Null(await tax.GetByIdAsync(id, TenantA));
    }

    [Fact]
    public async Task Repo_Query_FailsClosed_On_Empty_Tenant()
    {
        await using var db = Db();
        var accountId = Guid.NewGuid();
        db.ChartOfAccounts.Add(new ChartOfAccount
        {
            Id = accountId,
            AccountId = accountId,
            TenantId = TenantA,
            AccountCode = "2200",
            AccountName = "Output Tax",
            AccountType = "Liability",
            IsPostingAllowed = true,
            IsActive = true
        });
        var taxId = Guid.NewGuid();
        db.TaxCodes.Add(new TaxCode
        {
            Id = taxId,
            TaxCodeId = taxId,
            TenantId = TenantA,
            Code = "A1",
            TaxCodeName = "A",
            TaxType = "Sales",
            TaxRate = 10m,
            AccountId = accountId,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var repo = new TaxCodeRepository(db);
        Assert.Empty(await repo.GetAllAsync(Guid.Empty));
        Assert.Null(await repo.GetByCodeAsync("A1", Guid.Empty));
        Assert.Single(await repo.GetAllAsync(TenantA));
        Assert.Null(await repo.GetByIdAsync(taxId, TenantB));
    }
}
