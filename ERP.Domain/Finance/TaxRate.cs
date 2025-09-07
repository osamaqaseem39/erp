using ERP.Domain;

namespace ERP.Domain.Finance;

public class TaxRate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Type { get; set; } = "Percentage"; // Percentage, Fixed
    public string TaxType { get; set; } = string.Empty; // Sales, Purchase, VAT, etc.
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public new bool IsActive { get; set; } = true;
}