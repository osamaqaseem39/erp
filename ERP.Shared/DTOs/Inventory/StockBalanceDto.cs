using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class StockBalanceDto : BaseDto
{
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? ItemName { get; set; }
    public string? WarehouseName { get; set; }
}