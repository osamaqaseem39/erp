using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class EmployeeDto : BaseDto
{
    public new Guid? Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public new Guid? TenantId { get; set; }
    public Guid? CompanyId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? Salutation { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PreferredName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? MaritalStatus { get; set; }
    public string? JobTitle { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public string EmploymentType { get; set; } = "FullTime";
    public string? WorkEmail { get; set; }
    public string? PersonalEmail { get; set; }
    public string? WorkPhone { get; set; }
    public string? MobilePhone { get; set; }
    public Guid? WorkLocationId { get; set; }
    public DateTime? DateHired { get; set; }
    public DateTime? DateTerminated { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsRemote { get; set; }
    public Guid? EmploymentContractId { get; set; }
    public bool IsDeleted { get; set; }
    public new string? CreatedBy { get; set; }
    public new string? UpdatedBy { get; set; }
}

public class CreateEmployeeDto : CreateDto
{
    public Guid? CompanyId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? Salutation { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PreferredName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? MaritalStatus { get; set; }
    public string? JobTitle { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public string EmploymentType { get; set; } = "FullTime";
    public string? WorkEmail { get; set; }
    public string? PersonalEmail { get; set; }
    public string? WorkPhone { get; set; }
    public string? MobilePhone { get; set; }
    public Guid? WorkLocationId { get; set; }
    public DateTime? DateHired { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsRemote { get; set; }
    public Guid? EmploymentContractId { get; set; }
}

public class UpdateEmployeeDto : UpdateDto
{
    public Guid? CompanyId { get; set; }
    public string? Salutation { get; set; }
    public string? PreferredName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? MaritalStatus { get; set; }
    public string? JobTitle { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public string? EmploymentType { get; set; }
    public string? WorkEmail { get; set; }
    public string? PersonalEmail { get; set; }
    public string? WorkPhone { get; set; }
    public string? MobilePhone { get; set; }
    public Guid? WorkLocationId { get; set; }
    public DateTime? DateHired { get; set; }
    public DateTime? DateTerminated { get; set; }
    public string? Status { get; set; }
    public bool? IsRemote { get; set; }
    public Guid? EmploymentContractId { get; set; }
}