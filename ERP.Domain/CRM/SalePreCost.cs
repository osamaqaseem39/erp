using ERP.Domain;

namespace ERP.Domain.CRM;

public class SalePreCost : BaseEntity
{
    public string PreCostNumber { get; set; } = string.Empty;
    public Guid? SaleEnquiryId { get; set; }
    public Guid? QuoteId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime PreCostDate { get; set; }
    /// <summary>Draft → Approved | Rejected</summary>
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }

    public SaleEnquiry? SaleEnquiry { get; set; }
    public Quote? Quote { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<SalePreCostLine> Lines { get; set; } = new List<SalePreCostLine>();
}
