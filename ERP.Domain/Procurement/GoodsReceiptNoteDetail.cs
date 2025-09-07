using ERP.Domain;

namespace ERP.Domain.Procurement;

public class GoodsReceiptNoteDetail : BaseEntity
{
    public Guid GRNId { get; set; }
    public Guid ItemId { get; set; }
    public decimal QuantityReceived { get; set; }
    public Guid? BatchSerialId { get; set; } // nullable, if batch/serial tracked
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public new string CreatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual GoodsReceiptNote GoodsReceiptNote { get; set; } = null!;
    public virtual SCM.Item Item { get; set; } = null!;
    public virtual SCM.BatchSerialTracking? BatchSerialTracking { get; set; }
}
