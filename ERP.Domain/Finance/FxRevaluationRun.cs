using ERP.Domain;

namespace ERP.Domain.Finance;

public class FxRevaluationRun : BaseEntity
{
    public DateTime RunDate { get; set; }
    public string? RateSource { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? PostedJournalId { get; set; }
    public string? Notes { get; set; }

    public ICollection<FxRevaluationLine> Lines { get; set; } = new List<FxRevaluationLine>();
}

public class FxRevaluationLine : BaseEntity
{
    public Guid FxRevaluationRunId { get; set; }
    public Guid? AccountId { get; set; }
    public string? PartyType { get; set; }
    public Guid? PartyId { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ForeignBalance { get; set; }
    public decimal BookBalance { get; set; }
    public decimal RevaluedBalance { get; set; }
    public decimal UnrealizedGainLoss { get; set; }

    public FxRevaluationRun FxRevaluationRun { get; set; } = null!;
}
