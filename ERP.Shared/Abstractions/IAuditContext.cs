namespace ERP.Shared.Abstractions;

/// <summary>Who is making the current change, for the audit trail. Implemented from the HTTP request.</summary>
public interface IAuditContext
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
