using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class ItemVariantDto : BaseDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string VariantCode { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public decimal Weight { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateItemVariantDto : CreateDto
{
    public Guid ItemId { get; set; }
    public string VariantCode { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal? Price { get; set; }
    public decimal? Cost { get; set; }
    public decimal? Weight { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? Notes { get; set; }
    public bool? IsActive { get; set; } = true;
}

public class UpdateItemVariantDto : UpdateDto
{
    public string? VariantCode { get; set; }
    public string? VariantName { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal? Price { get; set; }
    public decimal? Cost { get; set; }
    public decimal? Weight { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string? Notes { get; set; }
    public bool? IsActive { get; set; }
}