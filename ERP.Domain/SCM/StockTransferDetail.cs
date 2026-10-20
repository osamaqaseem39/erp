using ERP.Domain;

namespace ERP.Domain.SCM;

public class StockTransferDetail : BaseEntity
{
    public Guid TransferId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? BatchSerialId { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }

    public virtual StockTransfer Transfer { get; set; } = null!;
    public virtual Item Item { get; set; } = null!;
    public virtual BatchSerialTracking? BatchSerial { get; set; }
    public virtual WarehouseLocation? FromLocation { get; set; }
    public virtual WarehouseLocation? ToLocation { get; set; }
}
