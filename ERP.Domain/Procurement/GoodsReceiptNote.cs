using ERP.Domain;

namespace ERP.Domain.Procurement;

public class GoodsReceiptNote : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid POId { get; set; }
    public Guid VendorId { get; set; }
    public string GRNNumber { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public Guid WarehouseId { get; set; }
    public Guid ReceivedByUserId { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Verified, Posted
    public string Remarks { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Core.Company Company { get; set; } = null!;
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual SCM.Warehouse Warehouse { get; set; } = null!;
    public virtual Core.User ReceivedByUser { get; set; } = null!;
    public virtual ICollection<GoodsReceiptNoteDetail> Details { get; set; } = new List<GoodsReceiptNoteDetail>();
}
