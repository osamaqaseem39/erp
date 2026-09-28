using System.Security.Claims;

namespace ERP.Api.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] SuperAdminRoles = ["SuperAdmin", "SUPER_ADMIN"];

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = ResolveTenantId(context);
        if (tenantId.HasValue)
        {
            context.Items["TenantId"] = tenantId.Value;
        }

        await _next(context);
    }

    private static Guid? ResolveTenantId(HttpContext context)
    {
        var user = context.User;
        Guid? claimTenantId = null;

        if (user.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = user.FindFirst("TenantId") ?? user.FindFirst("tenant_id");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var parsed))
            {
                claimTenantId = parsed;
            }
        }

        Guid? headerTenantId = null;
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader) &&
            Guid.TryParse(tenantIdHeader, out var headerParsed))
        {
            headerTenantId = headerParsed;
        }

        // Authenticated non-super-admin: JWT claim wins; header spoofing ignored
        if (user.Identity?.IsAuthenticated == true)
        {
            var isSuperAdmin = SuperAdminRoles.Any(r => user.IsInRole(r));
            if (!isSuperAdmin)
                return claimTenantId;

            // SuperAdmin may override via header
            return headerTenantId ?? claimTenantId;
        }

        // Anonymous (login/setup): allow header for tenant scoping only
        return headerTenantId;
    }
}
