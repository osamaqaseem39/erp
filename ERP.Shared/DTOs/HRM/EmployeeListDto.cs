using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class EmployeeListDto
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Department { get; set; }
    public Guid? ManagerId { get; set; }
    public string? WorkEmail { get; set; }
    public string? WorkPhone { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DateHired { get; set; }
    public bool IsActive { get; set; }
}