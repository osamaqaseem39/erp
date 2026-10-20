using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Projects;

public class ProjectDto : BaseDto
{
    public Guid? CompanyId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public Guid? ProjectManagerId { get; set; }
    public string? ProjectManagerName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string Status { get; set; } = "Planning";
    public string Priority { get; set; } = "Medium";
    public decimal Budget { get; set; }
    public decimal ActualCost { get; set; }
    public decimal Progress { get; set; }
    public string? Notes { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? CurrencyId { get; set; }
    public Guid? BranchId { get; set; }
}

public class CreateProjectDto : CreateDto
{
    public Guid? CompanyId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ProjectManagerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; } = "Planning";
    public string? Priority { get; set; } = "Medium";
    public decimal? Budget { get; set; }
    public string? Notes { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? CurrencyId { get; set; }
    public Guid? BranchId { get; set; }
}

public class UpdateProjectDto : UpdateDto
{
    public string? ProjectCode { get; set; }
    public string? ProjectName { get; set; }
    public string? Description { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ProjectManagerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public decimal? Progress { get; set; }
    public string? Notes { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? CurrencyId { get; set; }
    public Guid? BranchId { get; set; }
}