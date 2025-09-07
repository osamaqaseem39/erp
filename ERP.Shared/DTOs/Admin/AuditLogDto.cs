using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Admin;

public class AuditLogDto : BaseDto
{
    public new Guid? Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Notes { get; set; }
}

public class CreateAuditLogDto
{
    public Guid? TenantId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Notes { get; set; }
}