using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class BarcodeDto : BaseDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string BarcodeType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public Guid? VariantId { get; set; }
    public string? VariantName { get; set; }
}

public class CreateBarcodeDto : CreateDto
{
    public Guid ItemId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string BarcodeType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? VariantId { get; set; }
}

public class UpdateBarcodeDto : UpdateDto
{
    public string Barcode { get; set; } = string.Empty;
    public string BarcodeType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public Guid? VariantId { get; set; }
}