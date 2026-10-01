using ERP.Api.Errors;
using ERP.Infrastructure.Security;
using ERP.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERP.Api.Security.Permissions;

/// <summary>
/// Global check that the caller holds the permission an endpoint requires (see EndpointPermissions).
/// Runs after authentication. SuperAdmin passes every permission check. Endpoints whose permission
/// can't be determined are denied rather than allowed.
/// </summary>
public sealed class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly EndpointPermissions _endpoints;
    private readonly IEffectivePermissions _permissions;
    private readonly ILogger<PermissionAuthorizationFilter> _logger;

    public PermissionAuthorizationFilter(EndpointPermissions endpoints, IEffectivePermissions permissions, ILogger<PermissionAuthorizationFilter> logger)
    {
        _endpoints = endpoints;
        _permissions = permissions;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor action) return;
        var http = context.HttpContext;
        var rule = _endpoints.Resolve(action, http.Request.Method);

        switch (rule.Access)
        {
            case EndpointAccess.Anonymous:
            case EndpointAccess.SuperAdmin: // [Authorize(Roles = "SuperAdmin")] is checked by ASP.NET itself
                return;
        }

        if (http.User.Identity?.IsAuthenticated != true) return; // the fallback policy answers 401
        if (http.User.IsSuperAdmin()) return;

        switch (rule.Access)
        {
            case EndpointAccess.SignedIn:
                return;
            case EndpointAccess.Unmapped:
                _logger.LogError("Denied {Method} {Route}: no permission is declared for {Controller}.{Action}", rule.HttpMethod, rule.Route, rule.Controller, rule.Action);
                context.Result = Forbidden(http, "This action isn't available to your role.", null);
                return;
        }

        var granted = await _permissions.GetAsync(http.RequireTenantId(), http.User.RequireUserId(), http.RequestAborted);
        if (!granted.Contains(rule.Code!))
            context.Result = Forbidden(http, "You don't have permission to do this.", rule.Code);
    }

    private static ObjectResult Forbidden(HttpContext http, string message, string? code) => new(new ApiError
    {
        Status = 403,
        ErrorCode = ApiErrorCodes.Forbidden,
        Message = message,
        // The missing permission tells an administrator exactly what to grant.
        Errors = code == null ? null : new Dictionary<string, string[]> { ["permission"] = [code] },
        TraceId = http.TraceIdentifier,
    })
    { StatusCode = 403 };
}
