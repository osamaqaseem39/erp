using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Projects;

public class ProjectMemberDto : BaseDto
{
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string Role { get; set; } = "Member";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class CreateProjectMemberDto : CreateDto
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public string Role { get; set; } = "Member";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; } = "Active";
    public string? Notes { get; set; }
}

public class UpdateProjectMemberDto : UpdateDto
{
    public string? Role { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}
