using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Api.Mappers.HRM;

public class EmployeeMapper : BaseMapper<Employee, EmployeeDto>
{
    public override EmployeeDto Map(Employee source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        
        return new EmployeeDto
        {
            Id = source.Id,
            EmployeeId = source.Id,
            TenantId = source.TenantId,
            CompanyId = source.CompanyId,
            EmployeeCode = source.EmployeeCode,
            FirstName = source.FirstName,
            LastName = source.LastName,
            PreferredName = source.PreferredName,
            Gender = source.Gender,
            DateOfBirth = source.DateOfBirth,
            Nationality = source.Nationality,
            MaritalStatus = source.MaritalStatus,
            JobTitle = source.JobTitle,
            DepartmentId = source.DepartmentId,
            ManagerId = source.ManagerId,
            EmploymentType = source.EmploymentType,
            WorkEmail = source.WorkEmail,
            WorkPhone = source.WorkPhone,
            MobilePhone = source.MobilePhone,
            WorkLocationId = source.WorkLocationId,
            DateHired = source.DateHired,
            DateTerminated = source.DateTerminated,
            Status = source.Status,
            IsRemote = source.IsRemote,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override Employee Map(EmployeeDto destination)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        
        return new Employee
        {
            Id = destination.Id ?? destination.EmployeeId ?? Guid.NewGuid(),
            TenantId = destination.TenantId ?? Guid.Empty,
            CompanyId = destination.CompanyId,
            EmployeeCode = destination.EmployeeCode,
            FirstName = destination.FirstName,
            LastName = destination.LastName,
            PreferredName = destination.PreferredName,
            Gender = destination.Gender,
            DateOfBirth = destination.DateOfBirth,
            Nationality = destination.Nationality,
            MaritalStatus = destination.MaritalStatus,
            JobTitle = destination.JobTitle,
            DepartmentId = destination.DepartmentId,
            ManagerId = destination.ManagerId,
            EmploymentType = destination.EmploymentType,
            WorkEmail = destination.WorkEmail,
            WorkPhone = destination.WorkPhone,
            MobilePhone = destination.MobilePhone,
            WorkLocationId = destination.WorkLocationId,
            DateHired = destination.DateHired,
            DateTerminated = destination.DateTerminated,
            Status = destination.Status,
            IsRemote = destination.IsRemote,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}