using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class StockLedgerDto : BaseDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Guid? WarehouseLocationId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }
}