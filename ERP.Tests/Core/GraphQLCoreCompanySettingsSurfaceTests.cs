using ERP.Api.GraphQL.Core;
using ERP.Application.Core;
using ERP.Application.Core.Settings;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Admin;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ERP.Tests.Core;

public class GraphQLCoreCompanySettingsSurfaceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();

    private sealed class FixedTenant(Guid id) : ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }

    private sealed class PlainProtector : ISecretProtector
    {
        public string Protect(string plaintext) => plaintext;
        public string Unprotect(string protectedValue) => protectedValue;
    }

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void Surface_ExposesCompanySettingsSequencesAndDashboards()
    {
        var queries = typeof(CoreModuleQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(CoreModuleQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(CoreModuleMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(CoreModuleMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetCompanyProfile", queries);
        Assert.Contains("GetTenantContext", queries);
        Assert.Contains("GetSettingsCategory", queries);
        Assert.Contains("GetDocumentSequences", queries);
        Assert.Contains("GetCoreDashboard", queries);
        Assert.Contains("GetAdminDashboard", queries);

        Assert.Contains("UpdateCompanyProfile", mutations);
        Assert.Contains("UpdateSettingsCategory", mutations);
        Assert.Contains("UpdateDocumentSequence", mutations);
    }

    [Fact]
    public async Task GraphQL_CompanyProfile_Get_And_Update()
    {
        await using var db = Db();
        var company = new Company
        {
            TenantId = Tenant,
            CompanyName = "Acme",
            TaxNumber = "TN-1",
            Address = "1 Old St",
            ZipCode = "54000",
            Currency = "USD",
            TimeZone = "UTC"
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var settings = new CoreSettingsService(new SystemSettingRepository(db, new FixedTenant(Tenant)), new PlainProtector());
        var companies = new CompanyProfileService(new CompanyRepository(db), settings);
        var queries = new CoreModuleQueries();
        var mutations = new CoreModuleMutations();

        var profile = await queries.GetCompanyProfile(companies, company.Id, Tenant);
        Assert.Equal("Acme", profile.CompanyName);

        var updated = await mutations.UpdateCompanyProfile(companies, company.Id, Tenant, new UpdateCompanyProfileDto
        {
            CompanyName = "Acme Corp",
            Currency = "PKR",
            TimeZone = "Asia/Karachi",
            ContactPerson = "Aslam"
        });
        Assert.Equal("Acme Corp", updated.CompanyName);
        Assert.Equal("PKR", updated.Currency);
        Assert.Equal("Aslam", updated.ContactPerson);

        var ctx = await queries.GetTenantContext(companies, Tenant, company.Id);
        Assert.NotNull(ctx);
    }

    [Fact]
    public async Task GraphQL_SettingsCategory_Get_And_Update()
    {
        await using var db = Db();
        var settings = new CoreSettingsService(new SystemSettingRepository(db, new FixedTenant(Tenant)), new PlainProtector());
        var queries = new CoreModuleQueries();
        var mutations = new CoreModuleMutations();

        var prefs = await queries.GetSettingsCategory(settings, "preferences");
        Assert.Contains(prefs.Settings, s => s.Key == "preferences.locale");

        var updated = await mutations.UpdateSettingsCategory(settings, "security", new UpdateSettingsCategoryDto
        {
            Values = new Dictionary<string, string?> { ["security.sessionTimeoutMinutes"] = "90" }
        });
        Assert.Equal("90", updated.Settings.Single(s => s.Key == "security.sessionTimeoutMinutes").Value);
    }

    [Fact]
    public async Task GraphQL_CoreDashboard_Returns_Summary()
    {
        await using var db = Db();
        var dash = new CoreDashboardService(db, TimeProvider.System, NullLogger<CoreDashboardService>.Instance);
        var queries = new CoreModuleQueries();
        var summary = await queries.GetCoreDashboard(dash, Tenant);
        Assert.NotNull(summary);
        Assert.NotNull(summary.Users);
    }
}
