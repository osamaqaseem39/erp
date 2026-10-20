namespace ERP.Shared.DTOs.Manufacturing;

public class ScrapReasonDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class CreateScrapReasonDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class QualityCheckDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProductionOrderId { get; set; }
    public string CheckNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime? CheckedAt { get; set; }
    public string? Notes { get; set; }
    public List<QualityCheckLineDto> Lines { get; set; } = new();
}

public class QualityCheckLineDto
{
    public Guid? Id { get; set; }
    public Guid? ItemId { get; set; }
    public decimal QuantityInspected { get; set; }
    public decimal QuantityPassed { get; set; }
    public decimal QuantityFailed { get; set; }
    public string? Notes { get; set; }
}

public class CreateQualityCheckDto
{
    public Guid ProductionOrderId { get; set; }
    public string? Notes { get; set; }
    public List<QualityCheckLineDto> Lines { get; set; } = new();
}

public class PassFailQualityCheckDto
{
    public string? Notes { get; set; }
}

public class WorkCenterCapacityWarnDto
{
    public Guid WorkCenterId { get; set; }
    public string WorkCenterCode { get; set; } = string.Empty;
    public string WorkCenterName { get; set; } = string.Empty;
    public decimal CapacityHours { get; set; }
    public decimal LoadHours { get; set; }
    public bool Overloaded { get; set; }
    public DateTime WeekStartUtc { get; set; }
}

public class MaterialShortageDto
{
    public Guid ItemId { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal ShortageQuantity { get; set; }
}
