using ERP.Domain;

namespace ERP.Domain.Core;

public class Company : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string TimeZone { get; set; } = "UTC";
    
    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
