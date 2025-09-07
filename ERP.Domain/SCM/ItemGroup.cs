using ERP.Domain;

namespace ERP.Domain.SCM;

public class ItemGroup : BaseEntity
{
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public Guid? ParentGroupId { get; set; }
    public string? Description { get; set; }
    public new bool IsActive { get; set; } = true;
    public new Guid TenantId { get; set; }
    
    // Navigation properties
    public ItemGroup? ParentGroup { get; set; }
    public ICollection<ItemGroup> SubGroups { get; set; } = new List<ItemGroup>();
    public ICollection<Item> Items { get; set; } = new List<Item>();
}
