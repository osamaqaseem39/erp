using System.Security.Claims;
using ERP.Api.Security.Permissions;
using ERP.Infrastructure.Security;
using ERP.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;

namespace ERP.Tests.Security;

public class PermissionFilterTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly IActionDescriptorCollectionProvider Actions = EndpointPermissionTests.Actions();
    private static readonly EndpointPermissions Endpoints = new(Actions);

    private static ControllerActionDescriptor Action(string method, string route) =>
        Actions.ActionDescriptors.Items.OfType<ControllerActionDescriptor>().Single(a =>
            a.AttributeRouteInfo?.Template == route &&
            a.ActionConstraints!.OfType<Microsoft.AspNetCore.Mvc.ActionConstraints.HttpMethodActionConstraint>().Any(c => c.HttpMethods.Contains(method)));

    private static async Task<IActionResult?> Run(string method, string route, string[] granted, string? role = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, UserId.ToString()), new("TenantId", Tenant.ToString()) };
        if (role != null) claims.Add(new Claim(ClaimTypes.Role, role));
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) };
        http.Request.Method = method;
        http.Items["TenantId"] = Tenant;
        var context = new AuthorizationFilterContext(new ActionContext(http, new RouteData(), Action(method, route)), []);
        var filter = new PermissionAuthorizationFilter(Endpoints, new FixedPermissions(granted), NullLogger<PermissionAuthorizationFilter>.Instance);
        await filter.OnAuthorizationAsync(context);
        return context.Result;
    }

    [Fact]
    public async Task Allows_a_user_holding_the_permission()
    {
        Assert.Null(await Run("GET", "api/crm/customers", ["crm.customers.view"]));
    }

    [Fact]
    public async Task Refuses_with_403_naming_the_missing_permission()
    {
        var result = Assert.IsType<ObjectResult>(await Run("POST", "api/crm/customers", ["crm.customers.view"]));

        Assert.Equal(403, result.StatusCode);
        var error = Assert.IsType<ApiError>(result.Value);
        Assert.Equal(ApiErrorCodes.Forbidden, error.ErrorCode);
        Assert.Equal(["crm.customers.create"], error.Errors!["permission"]);
    }

    [Fact]
    public async Task View_only_vendor_may_list_but_not_create()
    {
        Assert.Null(await Run("GET", "api/procurement/vendors", ["procurement.vendors.view"]));
        var result = Assert.IsType<ObjectResult>(await Run("POST", "api/procurement/vendors", ["procurement.vendors.view"]));
        Assert.Equal(403, result.StatusCode);
        var error = Assert.IsType<ApiError>(result.Value);
        Assert.Equal(["procurement.vendors.create"], error.Errors!["permission"]);
    }

    [Fact]
    public async Task SuperAdmin_passes_every_permission_check()
    {
        Assert.Null(await Run("DELETE", "api/crm/customers/{id:guid}", [], role: "SuperAdmin"));
    }

    [Fact]
    public async Task Signed_in_only_endpoints_need_no_permission()
    {
        Assert.Null(await Run("GET", "api/core/context", []));
    }

    private sealed class FixedPermissions(string[] codes) : IEffectivePermissions
    {
        public Task<IReadOnlySet<string>> GetAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlySet<string>>(codes.ToHashSet());

        public void Invalidate(Guid tenantId) { }
    }
}
