namespace ERP.Shared.DTOs.Admin;

public class AuditLogPagedQuery : PagedRequest
{
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? Action { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    /// <summary>SuperAdmin only: include every tenant. Ignored for everyone else.</summary>
    public bool AllTenants { get; set; }
}

public class SystemLogPagedQuery : PagedRequest
{
    public string? LogLevel { get; set; }
    public string? Source { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    /// <summary>SuperAdmin only: include every tenant. Ignored for everyone else.</summary>
    public bool AllTenants { get; set; }
}
