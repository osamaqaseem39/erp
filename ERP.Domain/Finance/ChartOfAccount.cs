using ERP.Domain;

namespace ERP.Domain.Finance;

public class ChartOfAccount : BaseEntity
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // Asset/Liability/Equity/Revenue/Expense
    public Guid? ParentAccountId { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Remarks { get; set; }
    public bool IsPostingAllowed { get; set; } = true;
    
    // Navigation properties
    public ChartOfAccount? ParentAccount { get; set; }
    public ICollection<ChartOfAccount> ChildAccounts { get; set; } = new List<ChartOfAccount>();
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    public ICollection<BillLine> BillLines { get; set; } = new List<BillLine>();
    public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    public ICollection<FixedAsset> FixedAssets { get; set; } = new List<FixedAsset>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<TaxCode> TaxCodes { get; set; } = new List<TaxCode>();
}
