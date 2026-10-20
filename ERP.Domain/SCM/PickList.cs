using ERP.Domain;

namespace ERP.Domain.SCM;

public class PickList : BaseEntity
{
    public string PickNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string ReferenceDocType { get; set; } = "DispatchAdvice";
    public Guid? ReferenceDocId { get; set; }
    public string Status { get; set; } = "Open"; // Open | Picking | Completed | Cancelled
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<PickListLine> Lines { get; set; } = new List<PickListLine>();
}

public class PickListLine : BaseEntity
{
    public Guid PickListId { get; set; }
    public Guid ItemId { get; set; }
    public decimal QuantityRequested { get; set; }
    public decimal QuantityPicked { get; set; }
    public Guid? FromLocationId { get; set; }
    public string? Notes { get; set; }

    public PickList PickList { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public WarehouseLocation? FromLocation { get; set; }
}
