using ERP.Application.Interfaces.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

public class TimeTrackingService : ITimeTrackingService
{
    private readonly ITimeEntryRepository _repository;

    public TimeTrackingService(ITimeEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<TimeEntryDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<TimeEntryDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateTimeEntryDto dto, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var entity = MapToEntity(dto);
        entity.TenantId = tenantId;
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateTimeEntryDto dto, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<TimeEntryDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByEmployeeAsync(employeeId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<TimeEntryDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<decimal> GetTotalHoursAsync(Guid employeeId, DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return 0m;
        var entities = await _repository.GetByEmployeeAndDateRangeAsync(employeeId, startDate, endDate, tenantId);
        return entities.Sum(e => e.HoursWorked ?? 0m);
    }

    private static TimeEntryDto MapToDto(TimeEntry entity)
    {
        return new TimeEntryDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            EmployeeId = entity.EmployeeId,
            ProjectId = entity.ProjectId,
            WorkDate = entity.Date,
            StartTime = entity.ClockIn,
            EndTime = entity.ClockOut ?? DateTime.UtcNow,
            HoursWorked = entity.HoursWorked ?? 0m,
            Description = entity.Notes,
            IsOvertime = false,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static TimeEntry MapToEntity(CreateTimeEntryDto dto)
    {
        return new TimeEntry
        {
            TenantId = dto.TenantId,
            EmployeeId = dto.EmployeeId,
            ProjectId = dto.ProjectId,
            Date = dto.WorkDate ?? DateTime.UtcNow,
            ClockIn = dto.StartTime ?? DateTime.UtcNow,
            ClockOut = dto.EndTime,
            HoursWorked = dto.HoursWorked,
            Notes = dto.Description,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status,
            Source = "Manual",
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdateTimeEntryDto dto, TimeEntry entity)
    {
        if (dto.ProjectId.HasValue) entity.ProjectId = dto.ProjectId;
        if (dto.WorkDate.HasValue) entity.Date = dto.WorkDate.Value;
        if (dto.StartTime.HasValue) entity.ClockIn = dto.StartTime.Value;
        if (dto.EndTime.HasValue) entity.ClockOut = dto.EndTime.Value;
        if (dto.HoursWorked.HasValue) entity.HoursWorked = dto.HoursWorked;
        if (dto.Description != null) entity.Notes = dto.Description;
        if (dto.Status != null) entity.Status = dto.Status;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
