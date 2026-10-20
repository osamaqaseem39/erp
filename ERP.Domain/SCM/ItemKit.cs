using ERP.Domain;

namespace ERP.Domain.SCM;

public class ItemKit : BaseEntity
{
    public Guid KitItemId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public Item KitItem { get; set; } = null!;
    public ICollection<ItemKitComponent> Components { get; set; } = new List<ItemKitComponent>();
}

public class ItemKitComponent : BaseEntity
{
    public Guid ItemKitId { get; set; }
    public Guid ComponentItemId { get; set; }
    public decimal Quantity { get; set; }

    public ItemKit ItemKit { get; set; } = null!;
    public Item ComponentItem { get; set; } = null!;
}
