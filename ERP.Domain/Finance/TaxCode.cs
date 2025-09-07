using ERP.Domain;

namespace ERP.Domain.Finance;

public class TaxCode : BaseEntity
{
    public string TaxCodeName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TaxCodeDescription { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public string TaxType { get; set; } = string.Empty; // Sales, Purchase, VAT, GST
    public new bool IsActive { get; set; } = true;
    public Guid AccountId { get; set; }
    public Guid TaxCodeId { get; set; }
    
    // Navigation properties
    public ChartOfAccount Account { get; set; } = null!;
}