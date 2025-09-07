using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Projects;

public class CancelProjectDto
{
    public Guid ProjectId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}