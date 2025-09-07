using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class FixedAssetQuery : BaseQuery
{
    public string? AssetCode { get; set; }
    public string? AssetName { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? DepartmentId { get; set; }
    public DateTime? PurchaseDateFrom { get; set; }
    public DateTime? PurchaseDateTo { get; set; }
    public decimal? PurchaseValueFrom { get; set; }
    public decimal? PurchaseValueTo { get; set; }
    public bool? IsActive { get; set; }
}