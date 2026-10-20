using ERP.Domain;

namespace ERP.Domain.Manufacturing;

public class BillOfMaterialLine : BaseEntity
{
    public Guid BillOfMaterialId { get; set; }
    public int LineNumber { get; set; }
    public Guid ComponentItemId { get; set; }
    /// <summary>Quantity of component required per 1 finished good.</summary>
    public decimal QuantityPer { get; set; }
    public decimal ScrapPercent { get; set; }
    public string? Notes { get; set; }

    public BillOfMaterial BillOfMaterial { get; set; } = null!;
}
