using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class SerialNumberDto : BaseDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PurchaseDate { get; set; }
    public DateTime? SaleDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSerialNumberDto : CreateDto
{
    public Guid ItemId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public Guid? WarehouseId { get; set; }
    public string Status { get; set; } = "Available";
    public DateTime? PurchaseDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSerialNumberDto : UpdateDto
{
    public string SerialNumber { get; set; } = string.Empty;
    public Guid? WarehouseId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PurchaseDate { get; set; }
    public DateTime? SaleDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}