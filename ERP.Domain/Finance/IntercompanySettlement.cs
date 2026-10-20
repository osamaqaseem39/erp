using ERP.Domain;

namespace ERP.Domain.Finance;

public class IntercompanySettlement : BaseEntity
{
    public string SettlementNumber { get; set; } = string.Empty;
    public DateTime SettlementDate { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? PostedJournalId { get; set; }
    public string? Notes { get; set; }

    public ICollection<IntercompanyTransaction> Transactions { get; set; } = new List<IntercompanyTransaction>();
}
