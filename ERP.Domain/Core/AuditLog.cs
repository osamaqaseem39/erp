using ERP.Domain;

namespace ERP.Domain.Core;

public class AuditLog : BaseEntity
{
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty; // Login, UpdateProfile, AddInvoice, etc.
    public string EntityName { get; set; } = string.Empty; // Table/Module affected
    public string EntityId { get; set; } = string.Empty; // Row ID affected
    public string OldValues { get; set; } = string.Empty; // JSON
    public string NewValues { get; set; } = string.Empty; // JSON
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
