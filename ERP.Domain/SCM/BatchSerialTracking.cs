using ERP.Domain;

namespace ERP.Domain.SCM;

public class BatchSerialTracking : BaseEntity
{
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public string? BatchNo { get; set; }
    public string? SerialNo { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public string Status { get; set; } = "Available"; // Available, Reserved, Issued, Expired, Damaged
    
    // Navigation properties
    public virtual Item Item { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual ICollection<StockTransferDetail> StockTransferDetails { get; set; } = new List<StockTransferDetail>();
}
