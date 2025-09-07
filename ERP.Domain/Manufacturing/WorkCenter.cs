using ERP.Domain;

namespace ERP.Domain.Manufacturing;

public class WorkCenter : BaseEntity
{
    public Guid? CompanyId { get; set; }
    public string WorkCenterCode { get; set; } = string.Empty;
    public string WorkCenterName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string WorkCenterType { get; set; } = string.Empty; // Machine, Assembly, Testing
    public decimal Capacity { get; set; }
    public string CapacityUnit { get; set; } = "Hours"; // Hours, Units
    public new bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    
    // Navigation properties
    public ICollection<ProductionRun> ProductionRuns { get; set; } = new List<ProductionRun>();
}