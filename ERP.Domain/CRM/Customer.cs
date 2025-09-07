using ERP.Domain;

namespace ERP.Domain.CRM;

public class Customer : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string CustomerType { get; set; } = "Individual"; // Individual, Company
    public string Status { get; set; } = "Active"; // Active, Inactive, Suspended
    public string? Industry { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string? PaymentTerms { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    
    // Navigation properties
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}