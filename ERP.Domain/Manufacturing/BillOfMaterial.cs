using ERP.Domain;

namespace ERP.Domain.Manufacturing;

public class BillOfMaterial : BaseEntity
{
    public string BomNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid FinishedGoodItemId { get; set; }
    public string Version { get; set; } = "1";
    /// <summary>Draft → Active → Obsolete</summary>
    public string Status { get; set; } = "Draft";
    public bool IsDefault { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }

    public ICollection<BillOfMaterialLine> Lines { get; set; } = new List<BillOfMaterialLine>();
}
