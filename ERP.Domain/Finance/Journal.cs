using ERP.Domain;

namespace ERP.Domain.Finance;

public class Journal : BaseEntity
{
    public Guid JournalId { get; set; }
    /// <summary>Sales, Purchase, Bank, Adjustment, OpeningBalance, CashPayment, CashReceipt, BankPayment, BankReceipt, Journal</summary>
    public string JournalType { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public DateTime JournalDate { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public string? Description { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal TotalDebit { get; set; } = 0;
    public decimal TotalCredit { get; set; } = 0;
    /// <summary>Draft → Approved (Closed UX) → Posted; also Reversed, Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public string JournalNumber { get; set; } = string.Empty;
    public string? ChequeNumber { get; set; }
    /// <summary>Outstanding | Cleared | Bounced | Cancelled — set when ChequeNumber is present.</summary>
    public string? ChequeStatus { get; set; }
    public DateTime? ChequeStatusAt { get; set; }
    public string? ChequeStatusNotes { get; set; }
    public string? PaidBy { get; set; }
    public string? ReceivedBy { get; set; }
    public Guid? CashOrBankAccountId { get; set; }
    /// <summary>Free-text invoice/bill number shown on typed voucher forms.</summary>
    public string? InvoiceNumber { get; set; }
    /// <summary>Optional link table, e.g. Invoices / Bills.</summary>
    public string? EntityReferenceTable { get; set; }
    /// <summary>Optional linked AR invoice or AP bill id.</summary>
    public Guid? EntityReferenceId { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public bool IsLocked { get; set; } = false;
    /// <summary>Set on post when an Active fiscal year covers JournalDate.</summary>
    public Guid? FinancialYearId { get; set; }

    // Navigation properties
    public ChartOfAccount? CashOrBankAccount { get; set; }
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    public ICollection<DepreciationEntry> DepreciationEntries { get; set; } = new List<DepreciationEntry>();
}
