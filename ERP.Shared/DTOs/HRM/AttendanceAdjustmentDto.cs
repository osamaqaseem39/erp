using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class AttendanceAdjustmentDto
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid ApprovedByUserId { get; set; }
}