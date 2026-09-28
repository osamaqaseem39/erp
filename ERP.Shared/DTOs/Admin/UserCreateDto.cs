using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Admin;

public class CreateUserDto : CreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid? CompanyId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid> RoleIds { get; set; } = new();
}