using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Admin;

public class AssignPermissionsDto
{
    public Guid RoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}