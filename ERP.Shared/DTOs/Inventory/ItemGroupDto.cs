using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class ItemGroupDto : BaseDto
{
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentGroupId { get; set; }
    public string? ParentGroupName { get; set; }
    public int Level { get; set; }
    public string? Path { get; set; }
    public bool IsLeaf { get; set; }
    public string? Notes { get; set; }
}

public class CreateItemGroupDto : CreateDto
{
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentGroupId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateItemGroupDto : UpdateDto
{
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentGroupId { get; set; }
    public string? Notes { get; set; }
}