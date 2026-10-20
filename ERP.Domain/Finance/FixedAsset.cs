using ERP.Domain;

namespace ERP.Domain.Finance;

public class FixedAsset : BaseEntity
{
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string AssetCategory { get; set; } = string.Empty;
    public Guid? AssetCategoryId { get; set; }
    public Guid AccountId { get; set; }
    /// <summary>EF shadow formerly; kept in sync with AccountId for dual CoA navigations.</summary>
    public Guid GLAccountAccountId { get; set; }
    public Guid AssetId { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal CurrentValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime? DisposalDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Disposed, Under Maintenance
    public string CurrencyCode { get; set; } = string.Empty;
    
    // Navigation properties
    public ChartOfAccount Account { get; set; } = null!;
    public ChartOfAccount GLAccount { get; set; } = null!;
    public AssetCategory? Category { get; set; }
    public ICollection<DepreciationEntry> DepreciationEntries { get; set; } = new List<DepreciationEntry>();
    public ICollection<AssetDisposal> Disposals { get; set; } = new List<AssetDisposal>();
    public ICollection<AssetTransfer> Transfers { get; set; } = new List<AssetTransfer>();
}