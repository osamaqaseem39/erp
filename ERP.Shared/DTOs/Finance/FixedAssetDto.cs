using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class FixedAssetDto : BaseDto
{
    public Guid? AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? GLAccountId { get; set; }
    public string? GLAccountName { get; set; }
    public string? GLAccountCode { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public string? Supplier { get; set; }
    public string? SerialNumber { get; set; }
    public string? Location { get; set; }
    public string? Department { get; set; }
    public string Status { get; set; } = "Active";
    public decimal DepreciationRate { get; set; }
    public string? DepreciationMethod { get; set; }
    public int? UsefulLifeMonths { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public decimal? NetBookValue { get; set; }
    public DateTime? DisposedAt { get; set; }
    public string? DisposedBy { get; set; }
    public List<DepreciationEntryDto>? DepreciationEntries { get; set; }
    public string? Notes { get; set; }
}

public class CreateFixedAssetDto : CreateDto
{
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? GLAccountId { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? Supplier { get; set; }
    public string? SerialNumber { get; set; }
    public string? Location { get; set; }
    public string? Department { get; set; }
    public string? Status { get; set; } = "Active";
    public decimal? DepreciationRate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateFixedAssetDto : UpdateDto
{
    public Guid? AssetId { get; set; }
    public string? AssetCode { get; set; }
    public string? AssetName { get; set; }
    public string? AssetType { get; set; }
    public string? Description { get; set; }
    public Guid? GLAccountId { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? Supplier { get; set; }
    public string? SerialNumber { get; set; }
    public string? Location { get; set; }
    public string? Department { get; set; }
    public string? Status { get; set; }
    public decimal? DepreciationRate { get; set; }
    public string? Notes { get; set; }
}
