using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.CRM;

public class Quote : BaseEntity
{
    public string QuoteNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public DateTime QuoteDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime ValidUntil { get; set; }
    public Guid? OpportunityId { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Sent, Accepted, Rejected, Expired
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? ConvertedDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? CompanyId { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public int CreditDays { get; set; }
    public string? PaymentMode { get; set; }
    public Guid? PaymentTermId { get; set; }
    public decimal Freight { get; set; }
    public decimal Overhead { get; set; }
    public string? DiscountType { get; set; }
    public decimal DiscountAmount { get; set; }
    
    // Navigation properties
    public Customer? Customer { get; set; }
    public User? AssignedToUser { get; set; }
    public Opportunity? Opportunity { get; set; }
    public ICollection<QuoteLine> QuoteLines { get; set; } = new List<QuoteLine>();
}