using ERP.Domain;

namespace ERP.Domain.Core;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public new bool IsActive { get; set; } = true;
    public bool IsSystemRole { get; set; } = false;
    public string? Notes { get; set; }
    
    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}