using ERP.Domain;

namespace ERP.Domain.Procurement;

public class VendorBankAccount : BaseEntity
{
    public Guid VendorId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string SWIFTCode { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public bool IsPrimary { get; set; } = false;
    
    // Navigation properties
    public virtual Vendor Vendor { get; set; } = null!;
}
