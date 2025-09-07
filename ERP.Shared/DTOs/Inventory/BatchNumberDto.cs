using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class BatchNumberDto : BaseDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal CostPrice { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateBatchNumberDto : CreateDto
{
    public Guid ItemId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateBatchNumberDto : UpdateDto
{
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}