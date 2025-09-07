using ERP.Domain;

namespace ERP.Domain.Core;

public class Tenant : BaseEntity
{
    public string TenantName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty; // custom domain/subdomain
    public string Industry { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active, Suspended, Deleted
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public new bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<Setting> Settings { get; set; } = new List<Setting>();
}
