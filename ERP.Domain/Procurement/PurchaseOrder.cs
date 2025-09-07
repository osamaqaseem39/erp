using ERP.Domain;

namespace ERP.Domain.Procurement;

public class PurchaseOrder : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid? RFQId { get; set; } // nullable, if derived from RFQ
    public Guid VendorId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpectedDeliveryDate { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal TotalAmount { get; set; } = 0;
    public string PaymentTerms { get; set; } = string.Empty;
    public string DeliveryTerms { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Draft, Issued, PartiallyReceived, Completed, Cancelled
    public new string CreatedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Core.Company Company { get; set; } = null!;
    public virtual RequestForQuotation? RequestForQuotation { get; set; }
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual ICollection<PurchaseOrderDetail> Details { get; set; } = new List<PurchaseOrderDetail>();
    public virtual ICollection<GoodsReceiptNote> GoodsReceiptNotes { get; set; } = new List<GoodsReceiptNote>();
    public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
}
