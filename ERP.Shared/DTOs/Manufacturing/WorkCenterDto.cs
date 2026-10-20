using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Manufacturing;

public class WorkCenterDto : BaseDto
{
    public new Guid? Id { get; set; }
    public string WorkCenterCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string WorkCenterName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string WorkCenterType { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal Capacity { get; set; }
    public string CapacityUnit { get; set; } = string.Empty;
    public decimal WeeklyAvailableHours { get; set; } = 40m;
    public decimal Efficiency { get; set; }
    public string? Notes { get; set; }
    public new bool IsActive { get; set; }
}

public class CreateWorkCenterDto : CreateDto
{
    public Guid? CompanyId { get; set; }
    public string WorkCenterCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string WorkCenterName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string WorkCenterType { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? Capacity { get; set; }
    public string? CapacityUnit { get; set; }
    public decimal? WeeklyAvailableHours { get; set; }
    public decimal? Efficiency { get; set; }
    public string? Notes { get; set; }
    public bool? IsActive { get; set; } = true;
}

public class UpdateWorkCenterDto : UpdateDto
{
    public string? WorkCenterCode { get; set; }
    public string? Code { get; set; }
    public string? WorkCenterName { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? WorkCenterType { get; set; }
    public string? Location { get; set; }
    public decimal? Capacity { get; set; }
    public string? CapacityUnit { get; set; }
    public decimal? WeeklyAvailableHours { get; set; }
    public decimal? Efficiency { get; set; }
    public string? Notes { get; set; }
    public bool? IsActive { get; set; }
}