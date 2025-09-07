using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class LeaveCancellationDto : BaseDto
{
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}