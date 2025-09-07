using ERP.Domain;
using ERP.Domain.Core;

namespace ERP.Domain.SCM;

public class ItemCategory : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    
    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual ItemCategory? ParentCategory { get; set; }
    public virtual ICollection<ItemCategory> SubCategories { get; set; } = new List<ItemCategory>();
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
