using ERP.Domain;

namespace ERP.Domain.Core;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    /// <summary>Grants the permission when true; the table keeps denied rows too.</summary>
    public bool IsAllowed { get; set; } = true;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedBy { get; set; }
    public new bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}