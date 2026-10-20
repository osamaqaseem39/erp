using ERP.Domain;

namespace ERP.Domain.SCM;

public class PackOrder : BaseEntity
{
    public string PackNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Guid? PickListId { get; set; }
    public Guid? DispatchAdviceId { get; set; }
    public string Status { get; set; } = "Open"; // Open | Packed | Cancelled
    public string? Notes { get; set; }
    public DateTime? PackedAt { get; set; }
    public string? PackedBy { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public PickList? PickList { get; set; }
    public ICollection<PackOrderLine> Lines { get; set; } = new List<PackOrderLine>();
}

public class PackOrderLine : BaseEntity
{
    public Guid PackOrderId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? PackageCode { get; set; }
    public string? Notes { get; set; }

    public PackOrder PackOrder { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
