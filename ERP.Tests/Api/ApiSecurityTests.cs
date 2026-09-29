using System.Reflection;
using System.Security.Claims;
using ERP.Api.Controllers;
using ERP.Api.Controllers.Admin;
using ERP.Api.Errors;
using ERP.Api.Security;
using ERP.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace ERP.Tests.Api;

public class ApiSecurityTests
{
    [Fact]
    public void CurrentUser_ReadsTheClaimsTheJwtActuallyContains()
    {
        var tenant = Guid.NewGuid();
        var user = Guid.NewGuid();
        var company = Guid.NewGuid();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, user.ToString()), new Claim("CompanyId", company.ToString()), new Claim(ClaimTypes.Role, "SuperAdmin")],
                "test")),
        };
        context.Items["TenantId"] = tenant;

        Assert.Equal(tenant, context.RequireTenantId());
        Assert.Equal(user, context.User.RequireUserId());
        Assert.Equal(company, context.User.RequireCompanyId());
        Assert.True(context.User.IsSuperAdmin());
    }

    [Fact]
    public void CurrentUser_MissingValuesAreUnauthorized_NeverARandomOrEmptyId()
    {
        var context = new DefaultHttpContext();
        context.Items["TenantId"] = Guid.Empty;

        Assert.Throws<UnauthorizedException>(() => context.RequireTenantId());
        Assert.Throws<UnauthorizedException>(() => context.User.RequireUserId());
        Assert.Throws<UnauthorizedException>(() => context.User.RequireCompanyId());
        Assert.False(context.User.IsSuperAdmin());
    }

    [Fact]
    public void UnauthorizedAccessException_MapsTo401()
    {
        Assert.Equal(401, ApiErrorMapper.Map(new UnauthorizedAccessException("Tenant ID not found in token")).Status);
    }

    [Fact]
    public void TenantAdministration_IsSuperAdminOnly_ExceptTheTwoSignInLookups()
    {
        var authorize = typeof(TenantController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorize);
        Assert.Contains("SuperAdmin", authorize!.Roles);

        var anonymous = typeof(TenantController).GetMethods()
            .Where(m => m.GetCustomAttribute<AllowAnonymousAttribute>() != null)
            .Select(m => m.Name)
            .OrderBy(n => n);
        Assert.Equal(new[] { "GetByDomain", "GetLoginOptions" }, anonymous);
    }

    [Theory]
    [InlineData(typeof(AuditLogController))]
    [InlineData(typeof(SystemLogController))]
    public void LogControllers_ExposeNoWriteEndpoints(Type controller)
    {
        var writes = controller.GetMethods()
            .Where(m => m.GetCustomAttributes<HttpMethodAttribute>().Any(a => a.HttpMethods.Any(h => h != "GET")))
            .Select(m => m.Name);
        Assert.Empty(writes);
    }

    [Fact]
    public void NoControllerInventsIdentity()
    {
        // Guard against the old placeholders (Guid.NewGuid() / "system") coming back.
        var source = Directory.GetFiles(FindRepoPath("ERP.Api", "Controllers"), "*.cs", SearchOption.AllDirectories)
            .Where(f => File.ReadAllText(f).Contains("// Placeholder"))
            .Select(Path.GetFileName);
        Assert.Empty(source);
    }

    private static string FindRepoPath(string first, string second, [System.Runtime.CompilerServices.CallerFilePath] string thisFile = "")
    {
        // Resolve from this source file so it works wherever the test binaries are built.
        var dir = new DirectoryInfo(Path.GetDirectoryName(thisFile)!);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ERP.SaaS.sln"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, first, second);
    }
}
