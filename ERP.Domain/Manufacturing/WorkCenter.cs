using ERP.Domain;

namespace ERP.Domain.Manufacturing;

public class WorkCenter : BaseEntity
{
    public Guid? CompanyId { get; set; }
    public string WorkCenterCode { get; set; } = string.Empty;
    public string WorkCenterName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string WorkCenterType { get; set; } = string.Empty;
    public decimal Capacity { get; set; }
    public string CapacityUnit { get; set; } = "Hours";
    /// <summary>Weekly available hours for capacity warning (warn only).</summary>
    public decimal WeeklyAvailableHours { get; set; } = 40m;
    public new bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public ICollection<ProductionRun> ProductionRuns { get; set; } = new List<ProductionRun>();
}
