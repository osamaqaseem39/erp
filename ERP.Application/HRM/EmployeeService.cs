using ERP.Application.Interfaces;
using ERP.Shared.DTOs.HRM;
using ERP.Shared.DTOs;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces.HRM;
using ERP.Domain.HRM;

namespace ERP.Application.HRM;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmployeeDto> AddEmployeeAsync(CreateEmployeeDto dto)
    {
        var entity = MapToEntity(dto);
        var createdEntity = await _repository.AddAsync(entity);
        return MapToDto(createdEntity);
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(UpdateEmployeeDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) throw new KeyNotFoundException($"Employee with ID '{dto.Id}' not found.");

        MapToEntity(dto, entity);
        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task DeleteEmployeeAsync(Guid employeeId)
    {
        await _repository.DeleteAsync(employeeId);
    }

    public async Task<EmployeeDto> GetEmployeeByIdAsync(Guid employeeId)
    {
        var entity = await _repository.GetByIdAsync(employeeId);
        if (entity == null) throw new KeyNotFoundException($"Employee with ID '{employeeId}' not found.");
        return MapToDto(entity);
    }

    public async Task<PagedResult<EmployeeListDto>> GetEmployeesAsync(EmployeeQuery filter)
    {
        var entities = await _repository.GetAllAsync();
        var filteredEntities = entities.Where(e => 
            (string.IsNullOrEmpty(filter.SearchTerm) || 
             e.FirstName.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
             e.LastName.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
             e.EmployeeCode.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase)) &&
            (filter.DepartmentId == null || e.DepartmentId == filter.DepartmentId) &&
            (filter.Status == null || e.Status == filter.Status) &&
            !e.IsDeleted
        );

        var totalCount = filteredEntities.Count();
        var pagedEntities = filteredEntities
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        var employeeListDtos = pagedEntities.Select(MapToEmployeeListDto).ToList();

        return new PagedResult<EmployeeListDto>
        {
            Items = employeeListDtos,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<EmployeeDto> GetEmployeeByCodeAsync(string employeeCode)
    {
        var entity = await _repository.GetByCodeAsync(employeeCode);
        if (entity == null) throw new KeyNotFoundException($"Employee with code '{employeeCode}' not found.");
        return MapToDto(entity);
    }

    public async Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, Guid? excludeEmployeeId = null)
    {
        var entity = await _repository.GetByCodeAsync(employeeCode);
        return entity == null || (excludeEmployeeId != null && entity.Id == excludeEmployeeId);
    }

    private static EmployeeDto MapToDto(Employee entity)
    {
        return new EmployeeDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            EmployeeCode = entity.EmployeeCode,
            Salutation = entity.Salutation,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            PreferredName = entity.PreferredName,
            Gender = entity.Gender,
            DateOfBirth = entity.DateOfBirth,
            Nationality = entity.Nationality,
            MaritalStatus = entity.MaritalStatus,
            JobTitle = entity.JobTitle,
            DepartmentId = entity.DepartmentId,
            ManagerId = entity.ManagerId,
            EmploymentType = entity.EmploymentType,
            WorkEmail = entity.WorkEmail,
            PersonalEmail = entity.PersonalEmail,
            WorkPhone = entity.WorkPhone,
            MobilePhone = entity.MobilePhone,
            WorkLocationId = entity.WorkLocationId,
            DateHired = entity.DateHired,
            DateTerminated = entity.DateTerminated,
            Status = entity.Status,
            IsRemote = entity.IsRemote,
            EmploymentContractId = entity.EmploymentContractId,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static Employee MapToEntity(CreateEmployeeDto dto)
    {
        return new Employee
        {
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId,
            EmployeeCode = dto.EmployeeCode,
            Salutation = dto.Salutation,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PreferredName = dto.PreferredName,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            Nationality = dto.Nationality,
            MaritalStatus = dto.MaritalStatus,
            JobTitle = dto.JobTitle,
            DepartmentId = dto.DepartmentId,
            ManagerId = dto.ManagerId,
            EmploymentType = dto.EmploymentType,
            WorkEmail = dto.WorkEmail,
            PersonalEmail = dto.PersonalEmail,
            WorkPhone = dto.WorkPhone,
            MobilePhone = dto.MobilePhone,
            WorkLocationId = dto.WorkLocationId,
            DateHired = dto.DateHired,
            Status = dto.Status,
            IsRemote = dto.IsRemote,
            EmploymentContractId = dto.EmploymentContractId,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdateEmployeeDto dto, Employee entity)
    {
        entity.Salutation = dto.Salutation;
        entity.PreferredName = dto.PreferredName;
        entity.Gender = dto.Gender;
        entity.DateOfBirth = dto.DateOfBirth;
        entity.Nationality = dto.Nationality;
        entity.MaritalStatus = dto.MaritalStatus;
        entity.JobTitle = dto.JobTitle;
        entity.DepartmentId = dto.DepartmentId;
        entity.ManagerId = dto.ManagerId;
        entity.EmploymentType = dto.EmploymentType;
        entity.WorkEmail = dto.WorkEmail;
        entity.PersonalEmail = dto.PersonalEmail;
        entity.WorkPhone = dto.WorkPhone;
        entity.MobilePhone = dto.MobilePhone;
        entity.WorkLocationId = dto.WorkLocationId;
        entity.DateHired = dto.DateHired;
        entity.DateTerminated = dto.DateTerminated;
        entity.Status = dto.Status;
        entity.IsRemote = dto.IsRemote ?? entity.IsRemote;
        entity.EmploymentContractId = dto.EmploymentContractId;
        entity.UpdatedBy = dto.UpdatedBy;
    }

    private static EmployeeListDto MapToEmployeeListDto(Employee entity)
    {
        return new EmployeeListDto
        {
            Id = entity.Id,
            EmployeeCode = entity.EmployeeCode,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            JobTitle = entity.JobTitle,
            DepartmentId = entity.DepartmentId,
            ManagerId = entity.ManagerId,
            WorkEmail = entity.WorkEmail,
            WorkPhone = entity.WorkPhone,
            Status = entity.Status,
            DateHired = entity.DateHired,
            IsActive = entity.IsActive
        };
    }
}