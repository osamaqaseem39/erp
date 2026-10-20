namespace ERP.Domain.CRM;

public class SalePreCostLine : PackagingLineFields
{
    public Guid SalePreCostId { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal MaterialCost { get; set; }
    public decimal ProcessCost { get; set; }
    public decimal FohCost { get; set; }
    public decimal MarginPercent { get; set; }
    public decimal UnitRate { get; set; }
    public string? Notes { get; set; }

    public SalePreCost SalePreCost { get; set; } = null!;
}
