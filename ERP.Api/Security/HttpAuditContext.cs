using System.Security.Claims;
using ERP.Shared.Abstractions;

namespace ERP.Api.Security;

/// <summary>The signed-in caller of the current request, for the audit trail.</summary>
public sealed class HttpAuditContext : IAuditContext
{
    private readonly IHttpContextAccessor _accessor;

    public HttpAuditContext(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private HttpContext? Http => _accessor.HttpContext;

    public Guid? UserId =>
        Guid.TryParse(Http?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id != Guid.Empty ? id : null;

    public Guid? TenantId => Http?.Items.TryGetValue("TenantId", out var value) == true && value is Guid id && id != Guid.Empty ? id : null;

    public string? IpAddress => Http?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => Http?.Request.Headers.UserAgent.ToString();

    public bool IsSuperAdmin => Http?.User.IsSuperAdmin() == true;
}
