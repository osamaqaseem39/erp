using ERP.Domain;

namespace ERP.Domain.Core;

public class Subscription : BaseEntity
{
    public new Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Suspended, Cancelled, Expired
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty; // CreditCard, BankTransfer, PayPal
    public string? PaymentReference { get; set; }
    public bool IsAutoRenew { get; set; } = true;
    public DateTime? LastBillingDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
}