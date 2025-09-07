using System.Security.Claims;

namespace ERP.Api.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract tenant ID from JWT token or header
        var tenantId = GetTenantId(context);
        
        if (tenantId.HasValue)
        {
            context.Items["TenantId"] = tenantId.Value;
        }

        await _next(context);
    }

    private static Guid? GetTenantId(HttpContext context)
    {
        // Try to get tenant ID from JWT token claims
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = user.FindFirst("TenantId");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                return tenantId;
            }
        }

        // Try to get tenant ID from header
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
        {
            if (Guid.TryParse(tenantIdHeader, out var headerTenantId))
            {
                return headerTenantId;
            }
        }

        return null;
    }
}