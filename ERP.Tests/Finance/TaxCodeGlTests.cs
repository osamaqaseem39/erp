using ERP.Application.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Finance;

public class TaxCodeGlTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();
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

    private static async Task<Guid> AddPostingAccountAsync(AccountingService accounting, string code, string name)
    {
        var id = await accounting.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = code,
            AccountName = name,
            AccountType = "Liability",
            IsPostingAllowed = true
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);
        return id;
    }

    [Fact]
    public async Task Create_Requires_PostingGlAccount_And_Returns_GlLink()
    {
        await using var db = Db();
        var (tax, accounting) = Services(db);
        var glId = await AddPostingAccountAsync(accounting, "2200", "Output Tax");

        Assert.Equal(Guid.Empty, await tax.CreateAsync(new CreateTaxCodeDto
        {
            TaxCode = "VAT15",
            TaxName = "VAT 15%",
            TaxType = "Sales",
            TaxRate = 15,
            GLAccountId = null
        }, Tenant, User));

        var id = await tax.CreateAsync(new CreateTaxCodeDto
        {
            TaxCode = "VAT15",
            TaxName = "VAT 15%",
            TaxType = "Sales",
            TaxRate = 15,
            GLAccountId = glId,
            Description = "Standard VAT"
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var loaded = await tax.GetByIdAsync(id, Tenant);
        Assert.NotNull(loaded);
        Assert.Equal("VAT15", loaded!.TaxCode);
        Assert.Equal(15m, loaded.TaxRate);
        Assert.Equal(glId, loaded.GLAccountId);
        Assert.Equal("Output Tax", loaded.GLAccountName);
        Assert.True(loaded.IsActive);
    }

    [Fact]
    public async Task Create_Rejects_NonPosting_Or_Foreign_Account()
    {
        await using var db = Db();
        var (tax, accounting) = Services(db);

        var headerId = await accounting.AddChartOfAccountAsync(new CreateChartOfAccountDto
        {
            AccountCode = "2000",
            AccountName = "Liabilities",
            AccountType = "Liability",
            IsPostingAllowed = false
        }, Tenant, User);
        Assert.Equal(Guid.Empty, await tax.CreateAsync(new CreateTaxCodeDto
        {
            TaxCode = "T1", TaxName = "T1", TaxType = "Sales", TaxRate = 5, GLAccountId = headerId
        }, Tenant, User));

        var otherGl = Guid.NewGuid();
        db.ChartOfAccounts.Add(new ChartOfAccount
        {
            Id = otherGl,
            AccountId = otherGl,
            TenantId = OtherTenant,
            AccountCode = "2299",
            AccountName = "Other Tax",
            AccountType = "Liability",
            IsPostingAllowed = true,
            IsActive = true
        });
        await db.SaveChangesAsync();

        Assert.Equal(Guid.Empty, await tax.CreateAsync(new CreateTaxCodeDto
        {
            TaxCode = "T2", TaxName = "T2", TaxType = "Sales", TaxRate = 5, GLAccountId = otherGl
        }, Tenant, User));
    }

    [Fact]
    public async Task Update_Can_Retarget_Gl_And_Tenant_Isolation()
    {
        await using var db = Db();
        var (tax, accounting) = Services(db);
        var gl1 = await AddPostingAccountAsync(accounting, "2210", "VAT Output");
        var gl2 = await AddPostingAccountAsync(accounting, "2211", "VAT Output Alt");

        var id = await tax.CreateAsync(new CreateTaxCodeDto
        {
            TaxCode = "VAT", TaxName = "VAT", TaxType = "VAT", TaxRate = 10, GLAccountId = gl1
        }, Tenant, User);

        Assert.True(await tax.UpdateAsync(new UpdateTaxCodeDto
        {
            Id = id, GLAccountId = gl2, TaxRate = 12
        }, Tenant, User));

        var loaded = await tax.GetByIdAsync(id, Tenant);
        Assert.Equal(gl2, loaded!.GLAccountId);
        Assert.Equal(12m, loaded.TaxRate);
        Assert.Equal("VAT Output Alt", loaded.GLAccountName);

        Assert.Null(await tax.GetByIdAsync(id, OtherTenant));
        Assert.False(await tax.UpdateAsync(new UpdateTaxCodeDto { Id = id, TaxName = "x" }, OtherTenant, User));
        Assert.False(await tax.DeleteAsync(id, OtherTenant));
        Assert.True(await tax.DeleteAsync(id, Tenant));
        Assert.Null(await tax.GetByIdAsync(id, Tenant));
    }

    [Fact]
    public async Task Duplicate_Code_Rejected()
    {
        await using var db = Db();
        var (tax, accounting) = Services(db);
        var glId = await AddPostingAccountAsync(accounting, "2300", "Input Tax");

        Assert.NotEqual(Guid.Empty, await tax.CreateAsync(new CreateTaxCodeDto
        {
            TaxCode = "GST", TaxName = "GST", TaxType = "Purchase", TaxRate = 5, GLAccountId = glId
        }, Tenant, User));

        Assert.Equal(Guid.Empty, await tax.CreateAsync(new CreateTaxCodeDto
        {
            TaxCode = "GST", TaxName = "GST Dup", TaxType = "Purchase", TaxRate = 5, GLAccountId = glId
        }, Tenant, User));
    }
}
