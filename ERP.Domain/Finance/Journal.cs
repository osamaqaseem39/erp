using ERP.Domain;

namespace ERP.Domain.Finance;

public class Journal : BaseEntity
{
    public Guid JournalId { get; set; }
    public string JournalType { get; set; } = string.Empty; // Sales, Purchase, Bank, Adjustment, OpeningBalance
    public string? ReferenceNo { get; set; }
    public DateTime JournalDate { get; set; }
    public string? Description { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal TotalDebit { get; set; } = 0;
    public decimal TotalCredit { get; set; } = 0;
    public string Status { get; set; } = "Draft"; // Draft, Posted, Reversed, Cancelled
    public string JournalNumber { get; set; } = string.Empty;
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public bool IsLocked { get; set; } = false;
    
    // Navigation properties
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    public ICollection<DepreciationEntry> DepreciationEntries { get; set; } = new List<DepreciationEntry>();
}
