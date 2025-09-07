using ERP.Domain;

namespace ERP.Domain.Finance;

public class PaymentApplication : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal AppliedAmount { get; set; }
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } = "Applied"; // Applied, Reversed
    
    // Navigation properties
    public Payment Payment { get; set; } = null!;
    public Invoice Invoice { get; set; } = null!;
}