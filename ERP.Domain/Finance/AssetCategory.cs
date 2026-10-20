using ERP.Domain;

namespace ERP.Domain.Finance;

public class AssetCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UsefulLifeMonths { get; set; } = 60;
    public string DepreciationMethod { get; set; } = "StraightLine";
    public Guid? AssetAccountId { get; set; }
    public Guid? AccumDepAccountId { get; set; }
    public Guid? DepExpenseAccountId { get; set; }

    public ICollection<FixedAsset> FixedAssets { get; set; } = new List<FixedAsset>();
}
