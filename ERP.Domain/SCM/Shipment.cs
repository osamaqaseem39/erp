using ERP.Domain;

namespace ERP.Domain.SCM;

public class Shipment : BaseEntity
{
    public string ShipmentNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Guid? PackOrderId { get; set; }
    public Guid? DispatchAdviceId { get; set; }
    public string Status { get; set; } = "Draft"; // Draft | Shipped | Cancelled
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime? ShippedAt { get; set; }
    public string? ShippedBy { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public PackOrder? PackOrder { get; set; }
    public ICollection<ShipmentLine> Lines { get; set; } = new List<ShipmentLine>();
}

public class ShipmentLine : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }

    public Shipment Shipment { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
