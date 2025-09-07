using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class ItemAttributeDto : BaseDto
{
    public string AttributeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AttributeType { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
}

public class CreateItemAttributeDto : CreateDto
{
    public string AttributeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AttributeType { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool? IsRequired { get; set; } = false;
    public bool? IsActive { get; set; } = true;
}

public class UpdateItemAttributeDto : UpdateDto
{
    public string? AttributeName { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? AttributeType { get; set; }
    public string? DataType { get; set; }
    public bool? IsRequired { get; set; }
    public bool? IsActive { get; set; }
}