using ERP.Domain;

namespace ERP.Domain.Finance;

public class PaymentApplication : BaseEntity
{
    public Guid PaymentId { get; set; }
    /// <summary>AR target — exactly one of InvoiceId or BillId required.</summary>
    public Guid? InvoiceId { get; set; }
    /// <summary>AP target — exactly one of InvoiceId or BillId required.</summary>
    public Guid? BillId { get; set; }
    public decimal AppliedAmount { get; set; }
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } = "Applied"; // Applied, Reversed
    
    // Navigation properties
    public Payment Payment { get; set; } = null!;
    public Invoice? Invoice { get; set; }
    public Bill? Bill { get; set; }
}