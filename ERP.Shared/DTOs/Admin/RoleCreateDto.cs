using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Admin;

public class CreateRoleDto : CreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}