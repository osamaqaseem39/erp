using ERP.Domain;

namespace ERP.Domain.SCM;

public class ItemVariant : BaseEntity
{
    public Guid ItemId { get; set; }
    public string VariantCode { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public decimal CostPrice { get; set; } = 0;
    public decimal SellingPrice { get; set; } = 0;
    public new bool IsActive { get; set; } = true;
    public new Guid TenantId { get; set; }
    
    // Navigation properties
    public Item Item { get; set; } = null!;
    public ICollection<ItemVariantAttribute> ItemVariantAttributes { get; set; } = new List<ItemVariantAttribute>();
    public ICollection<Barcode> Barcodes { get; set; } = new List<Barcode>();
}
