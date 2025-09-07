using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class RoleDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public new bool IsActive { get; set; } = true;
    public bool IsSystemRole { get; set; } = false;
    public string? Notes { get; set; }
}

public class CreateRoleDto : CreateDto
{
    public string Name { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; } = false;
    public string? Notes { get; set; }
}

public class UpdateRoleDto : UpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public new bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}