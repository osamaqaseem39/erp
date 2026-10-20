using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Admin;

public class UserDto : BaseDto
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
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public new bool IsActive { get; set; }
    /// <summary>Assigned role ids (User Types in the reference product).</summary>
    public List<Guid> RoleIds { get; set; } = new();
    /// <summary>Assigned role names for list/detail display.</summary>
    public List<string> RoleNames { get; set; } = new();
}