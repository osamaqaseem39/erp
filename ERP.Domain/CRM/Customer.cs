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
    /// <summary>Payment due days (e.g. 30 for Net 30).</summary>
    public int CreditDays { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string? PaymentTerms { get; set; }
    /// <summary>Legacy single tax id; prefer Ntn/Strn/Cnic.</summary>
    public string? TaxId { get; set; }
    /// <summary>Filer / NonFiler / Exempt / etc.</summary>
    public string? TaxType { get; set; }
    public string? Ntn { get; set; }
    public string? Strn { get; set; }
    public string? Cnic { get; set; }
    public Guid? SalesmanId { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string CurrencyCode { get; set; } = "USD";

    public Salesman? Salesman { get; set; }
    // Navigation properties
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}