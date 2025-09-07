using ERP.Domain;

namespace ERP.Domain.Core;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public new bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    
    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}