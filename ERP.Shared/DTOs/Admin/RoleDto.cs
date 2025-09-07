using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Admin;

public class RoleDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public new bool IsActive { get; set; }
}