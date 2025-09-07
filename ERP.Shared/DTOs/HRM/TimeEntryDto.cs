using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class TimeEntryDto : BaseDto
{
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid? TaskId { get; set; }
    public string? TaskName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime WorkDate { get; set; }
    public decimal Hours { get; set; }
    public decimal HoursWorked { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "Draft";
    public bool IsBillable { get; set; }
    public bool IsOvertime { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Notes { get; set; }
}

public class CreateTimeEntryDto : CreateDto
{
    public Guid EmployeeId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? WorkDate { get; set; }
    public decimal? Hours { get; set; }
    public decimal? HoursWorked { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; } = "Draft";
    public bool? IsBillable { get; set; } = false;
    public decimal? HourlyRate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateTimeEntryDto : UpdateDto
{
    public Guid? EmployeeId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? WorkDate { get; set; }
    public decimal? Hours { get; set; }
    public decimal? HoursWorked { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public bool? IsBillable { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? Notes { get; set; }
}