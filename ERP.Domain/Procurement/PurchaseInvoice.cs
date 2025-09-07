using ERP.Domain;

namespace ERP.Domain.Procurement;

public class PurchaseInvoice : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid? POId { get; set; } // nullable, if directly invoiced
    public Guid VendorId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; } = 0;
    public decimal NetPayable { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Paid, PartiallyPaid, Cancelled
    
    // Navigation properties
    public virtual Core.Company Company { get; set; } = null!;
    public virtual PurchaseOrder? PurchaseOrder { get; set; }
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual ICollection<PurchaseInvoiceDetail> Details { get; set; } = new List<PurchaseInvoiceDetail>();
}
