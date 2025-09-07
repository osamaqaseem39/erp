using ERP.Domain;

namespace ERP.Domain.SCM;

public class ItemAttribute : BaseEntity
{
    public string AttributeName { get; set; } = string.Empty;
    public string AttributeType { get; set; } = string.Empty; // Text, Number, Date, List
    public bool IsRequired { get; set; } = false;
    public new bool IsActive { get; set; } = true;
    public new Guid TenantId { get; set; }
    
    // Navigation properties
    public ICollection<ItemVariantAttribute> ItemVariantAttributes { get; set; } = new List<ItemVariantAttribute>();
}
