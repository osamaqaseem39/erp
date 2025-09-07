using ERP.Domain;

namespace ERP.Domain.Finance;

public class BankAccount : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid BankId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string? IBAN { get; set; }
    public string? SWIFT { get; set; }
    public Guid GLAccountId { get; set; }
    
    // Navigation properties
    public ChartOfAccount GLAccount { get; set; } = null!;
    public Bank Bank { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<BankTransaction> BankTransactions { get; set; } = new List<BankTransaction>();
    public ICollection<BankReconciliation> BankReconciliations { get; set; } = new List<BankReconciliation>();
}
