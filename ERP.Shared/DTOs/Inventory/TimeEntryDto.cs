using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class TimeEntryDto : BaseDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid? TaskId { get; set; }
    public string? TaskName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Duration { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsBillable { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateTimeEntryDto : CreateDto
{
    public Guid EmployeeId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "Draft";
    public bool IsBillable { get; set; }
    public decimal HourlyRate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateTimeEntryDto : UpdateDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsBillable { get; set; }
    public decimal HourlyRate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}