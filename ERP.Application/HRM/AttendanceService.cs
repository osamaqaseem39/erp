using ERP.Application.Interfaces.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Interfaces.HRM;
using ERP.Shared.DTOs.HRM;

namespace ERP.Application.HRM;

public class AttendanceService : IAttendanceService
{
    private readonly ITimeEntryRepository _timeEntries;

    public AttendanceService(ITimeEntryRepository timeEntries) => _timeEntries = timeEntries;

    public async Task<AttendanceRecordDto> CheckInAsync(CheckInDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(tenantId));
        if (dto.EmployeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId is required.");

        var when = dto.CheckInTime == default ? DateTime.UtcNow : dto.CheckInTime.ToUniversalTime();
        var day = when.Date;

        var open = (await _timeEntries.GetByEmployeeAndDateRangeAsync(dto.EmployeeId, day, day.AddDays(1), tenantId))
            .FirstOrDefault(t =>
                t.ClockOut == null
                && string.Equals(t.Status, "Active", StringComparison.OrdinalIgnoreCase));
        if (open != null)
            throw new InvalidOperationException("Employee already has an open check-in for this day.");

        var entry = new TimeEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = dto.EmployeeId,
            ClockIn = when,
            Date = day,
            Location = dto.Location,
            Notes = dto.Notes,
            Status = "Active",
            Source = "Manual",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString(),
            IsActive = true
        };
        await _timeEntries.CreateAsync(entry);
        return Map(entry);
    }

    public async Task<AttendanceRecordDto> CheckOutAsync(CheckOutDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(tenantId));
        if (dto.EmployeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId is required.");

        var when = dto.CheckOutTime == default ? DateTime.UtcNow : dto.CheckOutTime.ToUniversalTime();
        var from = when.Date.AddDays(-1);
        var to = when.Date.AddDays(1);

        var open = (await _timeEntries.GetByEmployeeAndDateRangeAsync(dto.EmployeeId, from, to, tenantId))
            .Where(t => t.ClockOut == null)
            .OrderByDescending(t => t.ClockIn)
            .FirstOrDefault();
        if (open == null)
            throw new InvalidOperationException("No open check-in found to check out.");

        if (when < open.ClockIn)
            throw new ArgumentException("Check-out cannot be before check-in.");

        open.ClockOut = when;
        open.HoursWorked = Math.Round((decimal)(when - open.ClockIn).TotalHours, 2);
        open.Status = "Completed";
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            open.Notes = string.IsNullOrWhiteSpace(open.Notes) ? dto.Notes : $"{open.Notes}\n{dto.Notes}";
        open.UpdatedAt = DateTime.UtcNow;
        open.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _timeEntries.UpdateAsync(open);
        return Map(open);
    }

    public async Task<IEnumerable<AttendanceRecordDto>> GetEmployeeAttendanceAsync(
        Guid employeeId, Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        if (tenantId == Guid.Empty) return [];
        var rows = await _timeEntries.GetByEmployeeAndDateRangeAsync(
            employeeId, fromDate.Date, toDate.Date.AddDays(1), tenantId);
        return rows
            .OrderByDescending(t => t.ClockIn)
            .Select(Map)
            .ToList();
    }

    public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(
        Guid employeeId, Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        if (tenantId == Guid.Empty)
        {
            return new AttendanceSummaryDto
            {
                EmployeeId = employeeId,
                FromDate = fromDate.Date,
                ToDate = toDate.Date
            };
        }

        var rows = (await _timeEntries.GetByEmployeeAndDateRangeAsync(
            employeeId, fromDate.Date, toDate.Date.AddDays(1), tenantId)).ToList();

        var completed = rows.Where(t => t.ClockOut.HasValue).ToList();
        var totalHours = completed.Sum(t => t.HoursWorked ?? 0m);
        var daysPresent = completed.Select(t => t.Date.Date).Distinct().Count();
        return new AttendanceSummaryDto
        {
            EmployeeId = employeeId,
            FromDate = fromDate.Date,
            ToDate = toDate.Date,
            DaysPresent = daysPresent,
            OpenCheckIns = rows.Count(t => t.ClockOut == null),
            TotalEntries = rows.Count,
            TotalHours = Math.Round(totalHours, 2),
            AverageHoursPerDay = daysPresent == 0
                ? 0m
                : Math.Round(totalHours / daysPresent, 2)
        };
    }

    public async Task<AttendanceRecordDto> AdjustAttendanceAsync(AttendanceAdjustmentDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(tenantId));
        if (dto.EmployeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId is required.");
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Adjustment reason is required.");

        var day = dto.Date.Date;
        var existing = (await _timeEntries.GetByEmployeeAndDateRangeAsync(dto.EmployeeId, day, day.AddDays(1), tenantId))
            .OrderByDescending(t => t.ClockIn)
            .FirstOrDefault();

        if (existing == null)
        {
            if (!dto.CheckInTime.HasValue)
                throw new ArgumentException("CheckInTime is required when creating an adjustment.");

            var created = new TimeEntry
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EmployeeId = dto.EmployeeId,
                ClockIn = dto.CheckInTime.Value.ToUniversalTime(),
                ClockOut = dto.CheckOutTime?.ToUniversalTime(),
                Date = day,
                Notes = $"Adjusted: {dto.Reason}" + (string.IsNullOrWhiteSpace(dto.Notes) ? "" : $" — {dto.Notes}"),
                Status = dto.CheckOutTime.HasValue ? "Completed" : "Active",
                Source = "Manual",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId == Guid.Empty ? null : userId.ToString(),
                IsActive = true
            };
            if (created.ClockOut.HasValue)
                created.HoursWorked = Math.Round((decimal)(created.ClockOut.Value - created.ClockIn).TotalHours, 2);
            await _timeEntries.CreateAsync(created);
            return Map(created);
        }

        if (dto.CheckInTime.HasValue) existing.ClockIn = dto.CheckInTime.Value.ToUniversalTime();
        if (dto.CheckOutTime.HasValue)
        {
            existing.ClockOut = dto.CheckOutTime.Value.ToUniversalTime();
            existing.Status = "Completed";
        }
        if (existing.ClockOut.HasValue && existing.ClockOut < existing.ClockIn)
            throw new ArgumentException("Check-out cannot be before check-in.");
        if (existing.ClockOut.HasValue)
            existing.HoursWorked = Math.Round((decimal)(existing.ClockOut.Value - existing.ClockIn).TotalHours, 2);

        existing.Notes = string.IsNullOrWhiteSpace(existing.Notes)
            ? $"Adjusted: {dto.Reason}"
            : $"{existing.Notes}\nAdjusted: {dto.Reason}";
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            existing.Notes += $" — {dto.Notes}";
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _timeEntries.UpdateAsync(existing);
        return Map(existing);
    }

    private static AttendanceRecordDto Map(TimeEntry t) => new()
    {
        Id = t.Id,
        TenantId = t.TenantId,
        EmployeeId = t.EmployeeId,
        CheckInTime = t.ClockIn,
        CheckOutTime = t.ClockOut,
        Date = t.Date,
        HoursWorked = t.HoursWorked,
        Location = t.Location,
        Notes = t.Notes,
        Status = t.Status,
        Source = t.Source
    };
}
