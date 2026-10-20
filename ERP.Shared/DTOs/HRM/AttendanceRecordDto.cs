namespace ERP.Shared.DTOs.HRM;

public class AttendanceRecordDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public DateTime Date { get; set; }
    public decimal? HoursWorked { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Active";
    public string? Source { get; set; }
}

public class AttendanceSummaryDto
{
    public Guid EmployeeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int DaysPresent { get; set; }
    public int OpenCheckIns { get; set; }
    public int TotalEntries { get; set; }
    public decimal TotalHours { get; set; }
    public decimal AverageHoursPerDay { get; set; }
}
