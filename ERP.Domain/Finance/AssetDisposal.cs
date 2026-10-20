using ERP.Domain;

namespace ERP.Domain.Finance;

public class AssetDisposal : BaseEntity
{
    public Guid AssetId { get; set; }
    public DateTime DisposalDate { get; set; }
    public decimal Proceeds { get; set; }
    public decimal BookValue { get; set; }
    public decimal GainLoss { get; set; }
    public Guid? PostedJournalId { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }

    public FixedAsset Asset { get; set; } = null!;
}
