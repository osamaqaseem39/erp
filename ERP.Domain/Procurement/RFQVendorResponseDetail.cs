using ERP.Domain;

namespace ERP.Domain.Procurement;

public class RFQVendorResponseDetail : BaseEntity
{
    public Guid RFQResponseId { get; set; }
    public Guid ItemId { get; set; }
    public decimal QuotedUnitPrice { get; set; }
    public decimal Discount { get; set; } = 0; // percentage
    public decimal TaxAmount { get; set; } = 0;
    public decimal FinalUnitPrice { get; set; }
    public new string CreatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual RFQVendorResponse RFQVendorResponse { get; set; } = null!;
    public virtual SCM.Item Item { get; set; } = null!;
}
