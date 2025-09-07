using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class ItemCategoryDto : BaseDto
{
    public new Guid? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public new bool IsActive { get; set; }
}

public class CreateItemCategoryDto : CreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public new bool IsActive { get; set; } = true;
}

public class UpdateItemCategoryDto : UpdateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public new bool IsActive { get; set; }
}