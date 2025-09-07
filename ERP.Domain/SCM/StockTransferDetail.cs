using ERP.Domain;

namespace ERP.Domain.SCM;

public class StockTransferDetail : BaseEntity
{
    public Guid TransferId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? BatchSerialId { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Navigation properties
    public virtual StockTransfer Transfer { get; set; } = null!;
    public virtual Item Item { get; set; } = null!;
    public virtual BatchSerialTracking? BatchSerial { get; set; }
}
