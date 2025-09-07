using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.SCM;

public class StockBalance : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity => CurrentQuantity - ReservedQuantity;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual Item Item { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
}
