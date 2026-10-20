namespace ERP.Shared.DTOs.Inventory;

public class InventoryValuationDto
{
    public Guid TenantId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime AsOfDate { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalValue { get; set; }
    public List<InventoryValuationLineDto> Lines { get; set; } = new();
}

public class InventoryValuationLineDto
{
    public Guid ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal AverageRate { get; set; }
    public decimal TotalValue { get; set; }
}
