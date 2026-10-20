using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class StockTransferDto : BaseDto
{
    public Guid? Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public Guid FromWarehouseId { get; set; }
    public string? FromWarehouseName { get; set; }
    public Guid ToWarehouseId { get; set; }
    public string? ToWarehouseName { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public List<StockTransferDetailDto> Details { get; set; } = new();
}

public class CreateStockTransferDto : CreateDto
{
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public List<CreateStockTransferDetailDto> Details { get; set; } = new();
}

public class UpdateStockTransferDto : UpdateDto
{
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public List<UpdateStockTransferDetailDto> Details { get; set; } = new();
}

public class StockTransferDetailDto
{
    public new Guid Id { get; set; }
    public Guid StockTransferId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public string? Notes { get; set; }
}

public class CreateStockTransferDetailDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStockTransferDetailDto
{
    public new Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public string? Notes { get; set; }
}