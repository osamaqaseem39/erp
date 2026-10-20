using ERP.Domain;

namespace ERP.Domain.Manufacturing;

public class Routing : BaseEntity
{
    public string RoutingNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ItemId { get; set; }
    public string Version { get; set; } = "1";
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }

    public ICollection<RoutingStep> Steps { get; set; } = new List<RoutingStep>();
}

public class RoutingStep : BaseEntity
{
    public Guid RoutingId { get; set; }
    public int StepNumber { get; set; }
    public Guid? WorkCenterId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public decimal StandardTimeMinutes { get; set; }
    public string? Notes { get; set; }

    public Routing Routing { get; set; } = null!;
}

public class ProductionScrap : BaseEntity
{
    public Guid ProductionOrderId { get; set; }
    public Guid? ItemId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime ScrapDate { get; set; }
    public string? Reason { get; set; }

    public ProductionOrder? ProductionOrder { get; set; }
}
