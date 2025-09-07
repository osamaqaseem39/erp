using ERP.Domain;

namespace ERP.Domain.SCM;

public class ItemVariantAttribute : BaseEntity
{
    public Guid ItemVariantId { get; set; }
    public Guid ItemAttributeId { get; set; }
    public string AttributeValue { get; set; } = string.Empty;
    public new bool IsActive { get; set; } = true;
    public new Guid TenantId { get; set; }
    
    // Navigation properties
    public ItemVariant ItemVariant { get; set; } = null!;
    public ItemAttribute ItemAttribute { get; set; } = null!;
}
