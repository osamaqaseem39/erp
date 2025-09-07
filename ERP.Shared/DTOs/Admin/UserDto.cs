using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Admin;

public class UserDto : BaseDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public new bool IsActive { get; set; }
}