using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Projects;

public class CompleteProjectDto
{
    public Guid ProjectId { get; set; }
    public DateTime CompletionDate { get; set; }
    public string? Notes { get; set; }
}