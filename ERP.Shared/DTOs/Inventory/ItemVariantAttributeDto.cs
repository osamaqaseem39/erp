using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class ItemVariantAttributeDto : BaseDto
{
    public Guid ItemVariantId { get; set; }
    public string? ItemVariantName { get; set; }
    public Guid ItemAttributeId { get; set; }
    public string? ItemAttributeName { get; set; }
    public string AttributeValue { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class CreateItemVariantAttributeDto : CreateDto
{
    public Guid ItemVariantId { get; set; }
    public Guid ItemAttributeId { get; set; }
    public string AttributeValue { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateItemVariantAttributeDto : UpdateDto
{
    public Guid? ItemAttributeId { get; set; }
    public string? AttributeValue { get; set; }
    public string? Notes { get; set; }
}