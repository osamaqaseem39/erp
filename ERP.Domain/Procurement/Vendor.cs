using ERP.Domain;

namespace ERP.Domain.Procurement;

public class Vendor : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty; // VAT/GST/NTN
    public string PaymentTerms { get; set; } = string.Empty; // Net 30, Net 60, etc.
    public decimal CreditLimit { get; set; } = 0;
    
    // Navigation properties
    public virtual Core.Company Company { get; set; } = null!;
    public virtual ICollection<VendorBankAccount> BankAccounts { get; set; } = new List<VendorBankAccount>();
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
    public virtual ICollection<RFQVendorResponse> RFQResponses { get; set; } = new List<RFQVendorResponse>();
}
