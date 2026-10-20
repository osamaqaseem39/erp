using ERP.Domain;

namespace ERP.Domain.Procurement;

public class RequestForQuotation : BaseEntity
{
    public Guid? CompanyId { get; set; }
    public Guid? PRId { get; set; } // nullable, if RFQ generated from PR
    public Guid RequestorId { get; set; }
    public string RFQNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public decimal EstimatedValue { get; set; }
    public string? Terms { get; set; }
    public string? Notes { get; set; }
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Open, Awarded, Cancelled, Closed
    public new string CreatedBy { get; set; } = string.Empty;

    // Navigation properties
    public virtual Core.Company? Company { get; set; }
    public virtual PurchaseRequisition? PurchaseRequisition { get; set; }
    public virtual ICollection<RFQDetail> Details { get; set; } = new List<RFQDetail>();
    public virtual ICollection<RFQVendorResponse> VendorResponses { get; set; } = new List<RFQVendorResponse>();
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
