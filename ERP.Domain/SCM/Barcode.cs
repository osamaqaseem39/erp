using ERP.Domain;

namespace ERP.Domain.SCM;

public class Barcode : BaseEntity
{
    public Guid? ItemId { get; set; }
    public Guid? VariantId { get; set; }
    public string BarcodeValue { get; set; } = string.Empty;
    public string BarcodeType { get; set; } = string.Empty; // UPC, EAN, Code128, QR
    public bool IsPrimary { get; set; } = false;
    public new bool IsActive { get; set; } = true;
    public new Guid TenantId { get; set; }
    
    // Navigation properties
    public Item? Item { get; set; }
    public ItemVariant? ItemVariant { get; set; }
}
