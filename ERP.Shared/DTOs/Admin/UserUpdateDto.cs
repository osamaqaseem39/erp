using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Admin;

public class UpdateUserDto : UpdateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Phone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? ProfilePicture { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid> RoleIds { get; set; } = new();
}