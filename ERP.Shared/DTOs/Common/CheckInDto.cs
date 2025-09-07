using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Common;

public class CheckInDto
{
    public Guid EmployeeId { get; set; }
    public DateTime CheckInTime { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}