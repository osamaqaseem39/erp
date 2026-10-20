using ERP.Domain;

namespace ERP.Domain.SCM;

public class StockReservation : BaseEntity
{
    public string ReservationNumber { get; set; } = string.Empty;
    public Guid? SalesOrderId { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    public ICollection<StockReservationLine> Lines { get; set; } = new List<StockReservationLine>();
}

public class StockReservationLine : BaseEntity
{
    public Guid StockReservationId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public string Status { get; set; } = "Active";

    public StockReservation StockReservation { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}
