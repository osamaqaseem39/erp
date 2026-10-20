namespace ERP.Shared.DTOs.Inventory;

public class StockAverageRateDto
{
    public Guid ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime? AsOfDate { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AverageRate { get; set; }
    public decimal PreviousCostPrice { get; set; }
    public bool CostPriceUpdated { get; set; }
    public int LedgerEntryCount { get; set; }
}

public class CalculateStockAverageRateRequest
{
    public Guid ItemId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime? AsOfDate { get; set; }
    /// <summary>When true, writes the computed average onto Item.CostPrice.</summary>
    public bool UpdateItemCostPrice { get; set; }
}
