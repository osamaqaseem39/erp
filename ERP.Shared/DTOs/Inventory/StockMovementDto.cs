using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class StockMovementDto : BaseDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public Guid? FromWarehouseId { get; set; }
    public string? FromWarehouseName { get; set; }
    public Guid? ToWarehouseId { get; set; }
    public string? ToWarehouseName { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Reference { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime MovementDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateStockMovementDto : CreateDto
{
    public Guid ItemId { get; set; }
    public Guid? FromWarehouseId { get; set; }
    public Guid? ToWarehouseId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Reference { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime MovementDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateStockMovementDto : UpdateDto
{
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Reference { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime MovementDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}