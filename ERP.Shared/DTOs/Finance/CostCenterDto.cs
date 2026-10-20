using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class CostCenterDto : BaseDto
{
    public Guid? CostCenterId { get; set; }
    public string CostCenterCode { get; set; } = string.Empty;
    public string CostCenterName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCostCenterDto : CreateDto
{
    public string CostCenterCode { get; set; } = string.Empty;
    public string CostCenterName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsActive { get; set; } = true;
}

public class UpdateCostCenterDto : UpdateDto
{
    public Guid? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}