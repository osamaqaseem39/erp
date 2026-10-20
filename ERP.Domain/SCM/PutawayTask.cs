using ERP.Domain;

namespace ERP.Domain.SCM;

public class PutawayTask : BaseEntity
{
    public string TaskNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string ReferenceDocType { get; set; } = "GRN";
    public Guid? ReferenceDocId { get; set; }
    public string Status { get; set; } = "Open"; // Open | InProgress | Completed | Cancelled
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<PutawayTaskLine> Lines { get; set; } = new List<PutawayTaskLine>();
}

public class PutawayTaskLine : BaseEntity
{
    public Guid PutawayTaskId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? SuggestedLocationId { get; set; }
    public Guid? ActualLocationId { get; set; }
    public string? Notes { get; set; }

    public PutawayTask PutawayTask { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public WarehouseLocation? SuggestedLocation { get; set; }
    public WarehouseLocation? ActualLocation { get; set; }
}
