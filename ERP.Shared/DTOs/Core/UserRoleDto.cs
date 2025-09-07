using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class UserRoleDto : BaseDto
{
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUserRoleDto : CreateDto
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime? AssignedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool? IsActive { get; set; } = true;
}

public class UpdateUserRoleDto : UpdateDto
{
    public Guid? RoleId { get; set; }
    public DateTime? AssignedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool? IsActive { get; set; }
}