using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class DepreciationEntryDto : BaseDto
{
    public Guid FixedAssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public DateTime DepreciationDate { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public string Method { get; set; } = string.Empty;
    public int Period { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
}

public class DepreciationRunDto : BaseDto
{
    public DateTime RunDate { get; set; }
    public string Status { get; set; } = "Running";
    public int TotalAssets { get; set; }
    public int ProcessedAssets { get; set; }
    public int FailedAssets { get; set; }
    public decimal TotalDepreciationAmount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateDepreciationEntryDto : CreateDto
{
    public Guid FixedAssetId { get; set; }
    public DateTime DepreciationDate { get; set; }
    public decimal DepreciationAmount { get; set; }
    public string Method { get; set; } = string.Empty;
    public int Period { get; set; }
    public string Status { get; set; } = "Pending";
}

public class UpdateDepreciationEntryDto : UpdateDto
{
    public DateTime? DepreciationDate { get; set; }
    public decimal? DepreciationAmount { get; set; }
    public string? Method { get; set; }
    public int? Period { get; set; }
    public string? Status { get; set; }
}