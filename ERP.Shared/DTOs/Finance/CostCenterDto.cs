using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class CostCenterDto : BaseDto
{
    public string CostCenterCode { get; set; } = string.Empty;
    public string CostCenterName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCostCenterId { get; set; }
    public string? ParentCostCenterName { get; set; }
    public string? Manager { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class CreateCostCenterDto : CreateDto
{
    public string CostCenterCode { get; set; } = string.Empty;
    public string CostCenterName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCostCenterId { get; set; }
    public string? Manager { get; set; }
    public string? Department { get; set; }
    public bool? IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateCostCenterDto : UpdateDto
{
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public string? Description { get; set; }
    public Guid? ParentCostCenterId { get; set; }
    public string? Manager { get; set; }
    public string? Department { get; set; }
    public bool? IsActive { get; set; }
    public string? Notes { get; set; }
}