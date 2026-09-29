namespace ERP.Shared.Abstractions;

/// <summary>Who is making the current change (audit trail, tenant write guard). Implemented from the HTTP request.</summary>
public interface IAuditContext
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    /// <summary>SuperAdmin works across tenants and is exempt from the tenant write guard.</summary>
    bool IsSuperAdmin { get; }
}
