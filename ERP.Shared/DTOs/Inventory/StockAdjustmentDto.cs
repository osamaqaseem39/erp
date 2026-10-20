using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class StockAdjustmentDto : BaseDto
{
    public Guid? Id { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string AdjustmentType { get; set; } = "Physical";
    public string Status { get; set; } = "Draft";
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public List<StockAdjustmentDetailDto> Details { get; set; } = new();
}

public class CreateStockAdjustmentDto : CreateDto
{
    public string AdjustmentNumber { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public Guid WarehouseId { get; set; }
    public string AdjustmentType { get; set; } = "Physical";
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public List<CreateStockAdjustmentDetailDto> Details { get; set; } = new();
}

public class UpdateStockAdjustmentDto : UpdateDto
{
    public string AdjustmentNumber { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public Guid WarehouseId { get; set; }
    public string AdjustmentType { get; set; } = "Physical";
    public string Status { get; set; } = "Draft";
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public List<UpdateStockAdjustmentDetailDto> Details { get; set; } = new();
}

public class StockAdjustmentDetailDto
{
    public new Guid Id { get; set; }
    public Guid StockAdjustmentId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityCounted { get; set; }
    public decimal QuantityDifference { get; set; }
    public Guid? WarehouseLocationId { get; set; }
    public string? Notes { get; set; }
}

public class CreateStockAdjustmentDetailDto
{
    public Guid ItemId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityCounted { get; set; }
    public Guid? WarehouseLocationId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStockAdjustmentDetailDto
{
    public new Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityCounted { get; set; }
    public Guid? WarehouseLocationId { get; set; }
    public string? Notes { get; set; }
}