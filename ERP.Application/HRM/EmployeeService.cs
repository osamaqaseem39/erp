using ERP.Shared.DTOs.HRM;
using ERP.Shared.DTOs;
using ERP.Shared.Exceptions;
using ERP.Infrastructure.Interfaces.HRM;
using ERP.Infrastructure.Database;
using ERP.Domain.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.HRM;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly ERPDbContext _db;

    public EmployeeService(IEmployeeRepository repository, ERPDbContext db)
    {
        _repository = repository;
        _db = db;
    }

    public async Task<EmployeeDto> AddEmployeeAsync(CreateEmployeeDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");

        var entity = MapToEntity(dto);
        entity.TenantId = tenantId;
        entity.CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? userId.ToString() : dto.CreatedBy;
        var createdEntity = await _repository.AddAsync(entity);
        return MapToDto(createdEntity);
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(UpdateEmployeeDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");

        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) throw new KeyNotFoundException($"Employee with ID '{dto.Id}' not found.");

        MapToEntity(dto, entity);
        entity.UpdatedBy = string.IsNullOrWhiteSpace(dto.UpdatedBy) ? userId.ToString() : dto.UpdatedBy;
        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task DeleteEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");

        var entity = await _repository.GetByIdAsync(employeeId, tenantId);
        if (entity == null)
            throw new KeyNotFoundException($"Employee with ID '{employeeId}' not found.");

        var linked = await _db.Users.AsNoTracking()
            .Where(u => u.EmployeeId == employeeId && u.TenantId == tenantId)
            .Select(u => u.PrimaryEmail)
            .FirstOrDefaultAsync();
        if (linked != null)
            throw new ConflictException(
                $"Cannot delete this employee while linked to user {linked}. Unlink the user first.");

        await _repository.DeleteAsync(employeeId, tenantId);
    }

    public async Task<EmployeeDto> GetEmployeeByIdAsync(Guid employeeId, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new KeyNotFoundException($"Employee with ID '{employeeId}' not found.");

        var entity = await _repository.GetByIdAsync(employeeId, tenantId);
        if (entity == null) throw new KeyNotFoundException($"Employee with ID '{employeeId}' not found.");
        return MapToDto(entity);
    }

    public async Task<PagedResult<EmployeeListDto>> GetEmployeesAsync(EmployeeQuery filter, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            return new PagedResult<EmployeeListDto>
            {
                Items = [],
                TotalCount = 0,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        var entities = await _repository.GetAllAsync(tenantId);
        var filteredEntities = entities.Where(e =>
            (string.IsNullOrEmpty(filter.SearchTerm) ||
             e.FirstName.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
             e.LastName.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
             e.EmployeeCode.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase)) &&
            (filter.DepartmentId == null || e.DepartmentId == filter.DepartmentId) &&
            (filter.CompanyId == null || e.CompanyId == filter.CompanyId) &&
            (filter.Status == null || e.Status == filter.Status) &&
            !e.IsDeleted
        );

        var totalCount = filteredEntities.Count();
        var pageNumber = filter.PageNumber > 0 ? filter.PageNumber : Math.Max(1, filter.Page);
        var pageSize = filter.PageSize > 0 ? filter.PageSize : 50;
        var pagedEntities = filteredEntities
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<EmployeeListDto>
        {
            Items = pagedEntities.Select(MapToEmployeeListDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<EmployeeDto> GetEmployeeByCodeAsync(string employeeCode, Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new KeyNotFoundException($"Employee with code '{employeeCode}' not found.");

        var entity = await _repository.GetByCodeAsync(employeeCode, tenantId);
        if (entity == null) throw new KeyNotFoundException($"Employee with code '{employeeCode}' not found.");
        return MapToDto(entity);
    }

    public async Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, Guid tenantId, Guid? excludeEmployeeId = null)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByCodeAsync(employeeCode, tenantId);
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
