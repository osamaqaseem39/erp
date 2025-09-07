using ERP.Domain;

namespace ERP.Domain.Manufacturing;

public class ProductionRun : BaseEntity
{
    public Guid ProductionOrderId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal QuantityProduced { get; set; }
    public decimal QuantityRejected { get; set; }
    public string Status { get; set; } = "InProgress"; // InProgress, Completed, Cancelled
    public string? Notes { get; set; }
    
    // Navigation properties
    public ProductionOrder ProductionOrder { get; set; } = null!;
}