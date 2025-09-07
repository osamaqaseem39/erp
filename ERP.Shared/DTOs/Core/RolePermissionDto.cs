using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class RolePermissionDto : BaseDto
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public bool IsAllowed { get; set; } = true;
    public string RoleName { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
}

public class CreateRolePermissionDto : CreateDto
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public bool IsAllowed { get; set; } = true;
}

public class UpdateRolePermissionDto : UpdateDto
{   
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public bool IsAllowed { get; set; } = true;
}