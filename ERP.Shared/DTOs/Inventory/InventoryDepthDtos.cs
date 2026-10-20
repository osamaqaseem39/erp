using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class BrandDto : BaseDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateBrandDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateBrandDto : CreateBrandDto
{
    public Guid Id { get; set; }
    public bool? IsActive { get; set; }
}

public class WarehouseLocationDto : BaseDto
{
    public Guid WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LocationType { get; set; } = "Pick";
    public string? Zone { get; set; }
}

public class CreateWarehouseLocationDto
{
    public Guid WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LocationType { get; set; } = "Pick";
    public string? Zone { get; set; }
}

public class UpdateWarehouseLocationDto : CreateWarehouseLocationDto
{
    public Guid Id { get; set; }
    public bool? IsActive { get; set; }
}

public class StockReservationLineDto : BaseDto
{
    public Guid StockReservationId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public string Status { get; set; } = "Active";
}

public class StockReservationDto : BaseDto
{
    public string ReservationNumber { get; set; } = string.Empty;
    public Guid? SalesOrderId { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
    public List<StockReservationLineDto> Lines { get; set; } = [];
}

public class CreateStockReservationLineDto
{
    public Guid? SalesOrderId { get; set; }
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public string Status { get; set; } = "Active";
}

public class CreateStockReservationDto
{
    public Guid? SalesOrderId { get; set; }
    public string? ReservationNumber { get; set; }
    public string? Notes { get; set; }
    public List<CreateStockReservationLineDto> Lines { get; set; } = [];
}

public class ItemKitComponentDto : BaseDto
{
    public Guid ItemKitId { get; set; }
    public Guid ComponentItemId { get; set; }
    public decimal Quantity { get; set; }
}

public class ItemKitDto : BaseDto
{
    public Guid KitItemId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ItemKitComponentDto> Components { get; set; } = [];
}

public class CreateItemKitComponentDto
{
    public Guid ComponentItemId { get; set; }
    public decimal Quantity { get; set; }
}

public class CreateItemKitDto
{
    public Guid KitItemId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<CreateItemKitComponentDto> Components { get; set; } = [];
}

public class UpdateItemKitDto : CreateItemKitDto
{
    public Guid Id { get; set; }
    public bool? IsActive { get; set; }
}

public class CycleCountLineDto : BaseDto
{
    public Guid CycleCountId { get; set; }
    public Guid ItemId { get; set; }
    public Guid? WarehouseLocationId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityCounted { get; set; }
    public decimal QuantityVariance { get; set; }
    public string? Notes { get; set; }
}

public class CycleCountDto : BaseDto
{
    public string CountNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public DateTime CountDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public Guid? PostedStockAdjustmentId { get; set; }
    public List<CycleCountLineDto> Lines { get; set; } = [];
}

public class CreateCycleCountLineDto
{
    public Guid ItemId { get; set; }
    public Guid? WarehouseLocationId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityCounted { get; set; }
    public string? Notes { get; set; }
}

public class CreateCycleCountDto
{
    public Guid WarehouseId { get; set; }
    public DateTime CountDate { get; set; }
    public string? CountNumber { get; set; }
    public string? Notes { get; set; }
    public List<CreateCycleCountLineDto> Lines { get; set; } = [];
}

public class UpdateCycleCountDto : CreateCycleCountDto
{
    public Guid Id { get; set; }
}
