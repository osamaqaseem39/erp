using ERP.Domain.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Shared.Mappers.HRM;

public class TimeEntryMapper : IMapper<TimeEntry, TimeEntryDto>
{
    public TimeEntryDto ToDto(TimeEntry entity)
    {
        return new TimeEntryDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            EmployeeId = entity.EmployeeId,
            ProjectId = entity.ProjectId,
            WorkDate = entity.Date,
            StartTime = entity.ClockIn,
            EndTime = entity.ClockOut ?? DateTime.MinValue,
            HoursWorked = entity.HoursWorked ?? 0m,
            Description = entity.Notes,
            IsOvertime = false, // Default value
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    public TimeEntry ToEntity(TimeEntryDto dto)
    {
        return new TimeEntry
        {
            Id = dto.Id,
            TenantId = dto.TenantId,
            EmployeeId = dto.EmployeeId,
            ProjectId = dto.ProjectId,
            Date = dto.WorkDate,
            ClockIn = dto.StartTime,
            ClockOut = dto.EndTime == DateTime.MinValue ? null : dto.EndTime,
            HoursWorked = dto.HoursWorked,
            Notes = dto.Description,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            CreatedBy = dto.CreatedBy,
            UpdatedBy = dto.UpdatedBy,
            IsActive = dto.IsActive
        };
    }

    public TimeEntry MapToEntity(CreateTimeEntryDto dto)
    {
        return new TimeEntry
        {
            TenantId = dto.TenantId,
            EmployeeId = dto.EmployeeId,
            ProjectId = dto.ProjectId,
            Date = dto.WorkDate ?? DateTime.UtcNow,
            ClockIn = dto.StartTime ?? DateTime.UtcNow,
            ClockOut = dto.EndTime == DateTime.MinValue ? null : dto.EndTime,
            HoursWorked = dto.HoursWorked ?? 0m,
            Notes = dto.Description,
            Status = dto.Status ?? "Draft",
            CreatedBy = dto.CreatedBy
        };
    }

    public void MapToEntity(UpdateTimeEntryDto dto, TimeEntry entity)
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