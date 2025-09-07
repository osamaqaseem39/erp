using ERP.Domain;

namespace ERP.Domain.Finance;

public class Payment : BaseEntity
{
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string PaymentType { get; set; } = string.Empty; // Bank, Card, Stripe, Cash
    public string PaymentMethod { get; set; } = string.Empty; // Bank, Card, Stripe, Cash
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string CurrencyCode { get; set; } = "USD";
    public Guid? BankAccountId { get; set; }
    public Guid? RelatedInvoiceId { get; set; }
    public Guid? RelatedBillId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SupplierId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Reconciled
    public string? Reference { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedBy { get; set; }
    public Guid? CompanyId { get; set; }
    public ICollection<PaymentApplication> PaymentApplications { get; set; } = new List<PaymentApplication>();
    
    // Navigation properties
    public BankAccount? BankAccount { get; set; }
    public Invoice? RelatedInvoice { get; set; }
    public Bill? RelatedBill { get; set; }
    public ICollection<BankTransaction> BankTransactions { get; set; } = new List<BankTransaction>();
}
