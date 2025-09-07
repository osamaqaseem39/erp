using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class PermissionDto : BaseDto
{
    public new Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public new bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class CreatePermissionDto : CreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public new bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdatePermissionDto : UpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public new bool IsActive { get; set; }
    public string? Notes { get; set; }
}