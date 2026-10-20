using ERP.Domain;

namespace ERP.Domain.Finance;

public class RecurringJournal : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string JournalType { get; set; } = "Journal";
    public string Frequency { get; set; } = "Monthly";
    public DateTime NextRunDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ICollection<RecurringJournalLine> Lines { get; set; } = new List<RecurringJournalLine>();
}

public class RecurringJournalLine : BaseEntity
{
    public Guid RecurringJournalId { get; set; }
    public Guid AccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Narration { get; set; }

    public RecurringJournal RecurringJournal { get; set; } = null!;
}
