using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.CRM;

public class SalesOrder : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string SalesOrderNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Guid? QuoteId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? InvoicedDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Confirmed, Shipped, Delivered, Cancelled
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Currency { get; set; }
    public string? Terms { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? Notes { get; set; }
    public Guid? AssignedToUserId { get; set; }
    
    // Navigation properties
    public Customer? Customer { get; set; }
    public Quote? Quote { get; set; }
    public User? AssignedToUser { get; set; }
    public ICollection<SalesOrderLine> SalesOrderLines { get; set; } = new List<SalesOrderLine>();
}
