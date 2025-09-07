using ERP.Domain;

namespace ERP.Domain.Finance;

public class Invoice : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Guid CustomerId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal SubTotal { get; set; } = 0;
    public decimal TaxTotal { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;
    public decimal Balance { get; set; } = 0;
    public string Status { get; set; } = "Draft"; // Draft, Issued, Paid, Overdue, Cancelled
    public Guid? SalesOrderId { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public bool IsLocked { get; set; } = false;
    
    // Navigation properties
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
