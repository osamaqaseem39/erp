using ERP.Domain;

namespace ERP.Domain.Finance;

public class Bill : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid VendorId { get; set; }
    public Guid BillId { get; set; }
    public Guid SupplierId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime DueDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal SubTotal { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TaxTotal { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;
    public decimal Balance { get; set; } = 0;
    public string Status { get; set; } = "Draft"; // Draft, Issued, Paid, Overdue, Cancelled
    public Guid? PurchaseOrderId { get; set; }
    /// <summary>Operational Purchase invoice that created this AP bill.</summary>
    public Guid? SourcePurchaseInvoiceId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime? ApprovedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public bool IsLocked { get; set; } = false;
    
    // Navigation properties
    public ICollection<BillLine> BillLines { get; set; } = new List<BillLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
