using ERP.Domain;

namespace ERP.Domain.CRM;

public class SaleEnquiry : BaseEntity
{
    public string EnquiryNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public DateTime EnquiryDate { get; set; }
    /// <summary>Draft → Approved | Rejected</summary>
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public string? ReferenceNo { get; set; }

    public Customer? Customer { get; set; }
    public ICollection<SaleEnquiryLine> Lines { get; set; } = new List<SaleEnquiryLine>();
}
