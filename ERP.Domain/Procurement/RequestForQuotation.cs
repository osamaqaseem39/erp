using ERP.Domain;

namespace ERP.Domain.Procurement;

public class RequestForQuotation : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid? PRId { get; set; } // nullable, if RFQ generated from PR
    public Guid RequestorId { get; set; }
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Open"; // Open, Closed, Awarded, Cancelled
    public new string CreatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Core.Company Company { get; set; } = null!;
    public virtual PurchaseRequisition? PurchaseRequisition { get; set; }
    public virtual ICollection<RFQDetail> Details { get; set; } = new List<RFQDetail>();
    public virtual ICollection<RFQVendorResponse> VendorResponses { get; set; } = new List<RFQVendorResponse>();
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
