using ERP.Domain;

namespace ERP.Domain.SCM;

public class ReplenishmentTask : BaseEntity
{
    public string TaskNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string Status { get; set; } = "Open"; // Open | InProgress | Completed | Cancelled
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<ReplenishmentTaskLine> Lines { get; set; } = new List<ReplenishmentTaskLine>();
}

public class ReplenishmentTaskLine : BaseEntity
{
    public Guid ReplenishmentTaskId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public string? Notes { get; set; }

    public ReplenishmentTask ReplenishmentTask { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public WarehouseLocation? FromLocation { get; set; }
    public WarehouseLocation? ToLocation { get; set; }
}
