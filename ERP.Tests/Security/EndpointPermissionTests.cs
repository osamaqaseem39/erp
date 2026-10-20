using ERP.Api.Security.Permissions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ERP.Tests.Security;

public class EndpointPermissionTests
{
    private readonly ITestOutputHelper _output;

    public EndpointPermissionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    internal static EndpointPermissions Endpoints() => new(Actions());

    internal static IActionDescriptorCollectionProvider Actions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>(new TestHostEnvironment());
        services.AddControllers().AddApplicationPart(typeof(ERP.Api.Errors.ApiErrorMapper).Assembly);
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IActionDescriptorCollectionProvider>();
    }

    [Fact]
    public void Every_endpoint_requires_a_permission_or_declares_why_not()
    {
        var rules = Endpoints().AllRules;
        Assert.NotEmpty(rules);
        var unmapped = rules.Where(r => r.Access == EndpointAccess.Unmapped)
            .Select(r => $"{r.HttpMethod} {r.Route} ({r.Controller}.{r.Action})").ToList();
        foreach (var line in unmapped) _output.WriteLine(line);
        var dump = Environment.GetEnvironmentVariable("ERP_DUMP_PERMISSIONS");
        if (dump != null)
            File.WriteAllLines(dump, rules.OrderBy(r => r.Route).Select(r => $"{r.Access}	{r.Code}	{r.HttpMethod} {r.Route}	{r.Controller}"));
        Assert.Empty(unmapped);
    }

    [Fact]
    public void Codes_have_module_resource_and_action()
    {
        foreach (var rule in Endpoints().AllRules.Where(r => r.Access == EndpointAccess.Permission))
            Assert.Matches(@"^[a-z]+\.[a-zA-Z]+\.[a-zA-Z]+$", rule.Code!);
    }

    [Fact]
    public void Known_endpoints_map_to_expected_codes()
    {
        var rules = Endpoints().AllRules;
        string Code(string method, string route) =>
            rules.Single(r => r.HttpMethod == method && r.Route.Equals(route, StringComparison.OrdinalIgnoreCase)).Code!;

        Assert.Equal("crm.customers.view", Code("GET", "api/crm/customers"));
        Assert.Equal("crm.customers.create", Code("POST", "api/crm/customers"));
        Assert.Equal("crm.customers.delete", Code("DELETE", "api/crm/customers/{id:guid}"));
        Assert.Equal("admin.roles.assign", Code("POST", "api/admin/roles/{id}/permissions"));
        Assert.Equal("finance.chartOfAccounts.view", Code("GET", "api/finance/chartofaccounts"));
        Assert.Equal("projects.projects.start", Code("POST", "api/projects/{id}/start"));
        Assert.Equal("admin.permissions.view", Code("GET", "api/admin/permissions/catalog"));
        Assert.Equal("procurement.vendors.view", Code("GET", "api/procurement/vendors"));
        Assert.Equal("procurement.vendors.create", Code("POST", "api/procurement/vendors"));
        Assert.Equal("admin.emails.view", Code("GET", "api/admin/emails"));
        Assert.Contains(rules, r => r.Route == "api/core/context" && r.Access == EndpointAccess.SignedIn);
        Assert.Contains(rules, r => r.Route == "api/admin/permissions/seed" && r.Access == EndpointAccess.SuperAdmin);
    }

    [Theory]
    [InlineData("api/crm/customers", "GET", null, "crm.customers.view")]
    [InlineData("api/crm/customers/{id}", "PUT", null, "crm.customers.update")]
    [InlineData("api/crm/sales-orders/{id}", "PATCH", null, "crm.salesOrders.update")]
    [InlineData("api/crm/leads/{id}/convert", "POST", null, "crm.leads.convert")]
    [InlineData("api/crm/leads/{id}/convert", "GET", null, "crm.leads.view")]
    [InlineData("api/crm/leads/search", "POST", null, "crm.leads.create")]
    [InlineData("api/projects/{id}", "DELETE", "projects.projects", "projects.projects.delete")]
    [InlineData("api/reports/sales/summary", "GET", "reports.reports", "reports.reports.view")]
    [InlineData("api/finance/chart_of_accounts/{id}/close-period", "POST", null, "finance.chartOfAccounts.closePeriod")]
    public void Convention_derives_codes(string route, string method, string? resource, string expected)
    {
        Assert.Equal(expected, PermissionConvention.Derive(route, method, resource));
    }

    [Theory]
    [InlineData("api/projects/{id}")]
    [InlineData("api/core")]
    public void Convention_refuses_to_guess(string route)
    {
        Assert.Null(PermissionConvention.Derive(route, "GET", null));
    }

    private sealed class TestHostEnvironment : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = "";
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
        public string ApplicationName { get; set; } = "ERP.Api";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public string EnvironmentName { get; set; } = "Testing";
    }
}
