using ERP.Domain;

namespace ERP.Domain.SCM;

public class ItemPricing : BaseEntity
{
    public Guid ItemId { get; set; }
    public string PriceListName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal MinQuantity { get; set; } = 1;
    public decimal? MaxQuantity { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public new bool IsActive { get; set; } = true;
    public new Guid TenantId { get; set; }
    
    // Navigation properties
    public Item Item { get; set; } = null!;
}
