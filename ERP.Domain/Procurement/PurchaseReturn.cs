using ERP.Domain;

namespace ERP.Domain.Procurement;

public class PurchaseReturn : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public Guid VendorId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? PurchaseInvoiceId { get; set; }
    public Guid? WarehouseId { get; set; }
    /// <summary>PurchaseDebitNote id created and posted on Complete.</summary>
    public Guid? DebitNoteId { get; set; }
    public string Reason { get; set; } = string.Empty;
    /// <summary>Pending → Approved → Completed | Cancelled</summary>
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public Vendor? Vendor { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public ICollection<PurchaseReturnLine> Lines { get; set; } = new List<PurchaseReturnLine>();
}
