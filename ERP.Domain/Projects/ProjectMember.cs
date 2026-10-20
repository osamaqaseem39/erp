using ERP.Domain;
using ERP.Domain.HRM;

namespace ERP.Domain.Projects;

public class ProjectMember : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public string Role { get; set; } = "Member";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    public Project Project { get; set; } = null!;
    public Employee? Employee { get; set; }
}
