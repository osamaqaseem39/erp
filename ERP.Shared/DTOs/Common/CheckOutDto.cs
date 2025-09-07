using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Common;

public class CheckOutDto
{
    public Guid EmployeeId { get; set; }
    public DateTime CheckOutTime { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}