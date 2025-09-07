using ERP.Domain;

namespace ERP.Domain.Finance;

public class Bank : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string? SwiftCode { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public new bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    public virtual ICollection<BankTransaction> BankTransactions { get; set; } = new List<BankTransaction>();
}