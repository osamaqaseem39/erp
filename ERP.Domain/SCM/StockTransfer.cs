using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.SCM;

public class StockTransfer : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public DateTime TransferDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, InTransit, Completed, Cancelled
    public string? ApprovedBy { get; set; }
    
    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual Warehouse FromWarehouse { get; set; } = null!;
    public virtual Warehouse ToWarehouse { get; set; } = null!;
    public virtual ICollection<StockTransferDetail> StockTransferDetails { get; set; } = new List<StockTransferDetail>();
}
