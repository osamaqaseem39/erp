using ERP.Domain;

namespace ERP.Domain.Procurement;

public class PurchaseRequisition : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid RequestorId { get; set; }
    public Guid? DepartmentId { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime RequiredByDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected, ConvertedToPO
    public string Remarks { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Core.Company Company { get; set; } = null!;
    public virtual Core.User RequestedByUser { get; set; } = null!;
    public virtual ICollection<PurchaseRequisitionDetail> Details { get; set; } = new List<PurchaseRequisitionDetail>();
    public virtual ICollection<RequestForQuotation> RFQs { get; set; } = new List<RequestForQuotation>();
}
