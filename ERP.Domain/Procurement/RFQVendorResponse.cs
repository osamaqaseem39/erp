using ERP.Domain;

namespace ERP.Domain.Procurement;

public class RFQVendorResponse : BaseEntity
{
    public Guid RFQId { get; set; }
    public Guid VendorId { get; set; }
    public DateTime ResponseDate { get; set; } = DateTime.UtcNow;
    public decimal TotalQuotedAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentTerms { get; set; } = string.Empty;
    public int DeliveryTime { get; set; } // in days
    public string Status { get; set; } = "Submitted"; // Submitted, Shortlisted, Rejected, Accepted
    public new string CreatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual RequestForQuotation RequestForQuotation { get; set; } = null!;
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual ICollection<RFQVendorResponseDetail> Details { get; set; } = new List<RFQVendorResponseDetail>();
}
