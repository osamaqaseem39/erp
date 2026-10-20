using ERP.Domain;

namespace ERP.Domain.Finance;

public class AssetTransfer : BaseEntity
{
    public Guid AssetId { get; set; }
    public DateTime TransferDate { get; set; }
    public Guid? FromCostCenterId { get; set; }
    public Guid? ToCostCenterId { get; set; }
    public Guid? FromBranchId { get; set; }
    public Guid? ToBranchId { get; set; }
    public string? Notes { get; set; }

    public FixedAsset Asset { get; set; } = null!;
}
