using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class StockLevelDto : BaseDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
    public decimal QuantityOnOrder { get; set; }
    public decimal QuantityCommitted { get; set; }
    public decimal AverageCost { get; set; }
    public decimal LastCost { get; set; }
    public DateTime LastMovementDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateStockLevelDto : CreateDto
{
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityOnOrder { get; set; }
    public decimal QuantityCommitted { get; set; }
    public decimal AverageCost { get; set; }
    public decimal LastCost { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateStockLevelDto : UpdateDto
{
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityOnOrder { get; set; }
    public decimal QuantityCommitted { get; set; }
    public decimal AverageCost { get; set; }
    public decimal LastCost { get; set; }
    public bool IsActive { get; set; }
}