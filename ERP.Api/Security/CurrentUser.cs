using System.Security.Claims;
using ERP.Shared.Exceptions;

namespace ERP.Api.Security;

/// <summary>
/// The one way controllers read who is calling. Tenant comes from TenantMiddleware (JWT "TenantId";
/// SuperAdmin may switch tenant with X-Tenant-Id), user from the NameIdentifier claim, company from "CompanyId".
/// Missing values are a 401, never a random or empty id.
/// </summary>
public static class CurrentUser
{
    public const string TenantIdClaim = "TenantId";
    public const string CompanyIdClaim = "CompanyId";
    public static readonly string[] SuperAdminRoles = ["SuperAdmin", "SUPER_ADMIN"];

    public static Guid RequireTenantId(this HttpContext context)
    {
        if (context.Items.TryGetValue("TenantId", out var value) && value is Guid id && id != Guid.Empty)
            return id;
        throw new UnauthorizedException("Your session has no tenant. Please sign in again.");
    }

    public static Guid RequireUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        if (Guid.TryParse(raw, out var id) && id != Guid.Empty)
            return id;
        throw new UnauthorizedException("Your session has no user. Please sign in again.");
    }

    public static Guid RequireCompanyId(this ClaimsPrincipal user)
    {
        if (Guid.TryParse(user.FindFirstValue(CompanyIdClaim), out var id) && id != Guid.Empty)
            return id;
        throw new UnauthorizedException("Your account is not linked to a company. Sign out and in again, or ask an administrator.");
    }

    public static bool IsSuperAdmin(this ClaimsPrincipal user) => SuperAdminRoles.Any(user.IsInRole);
}
