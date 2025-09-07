using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class InventoryValuationMethodDto : BaseDto
{
    public string MethodName { get; set; } = string.Empty;
    public string MethodCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

public class CreateInventoryValuationMethodDto : CreateDto
{
    public string MethodName { get; set; } = string.Empty;
    public string MethodCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public bool? IsDefault { get; set; } = false;
    public bool? IsActive { get; set; } = true;
}

public class UpdateInventoryValuationMethodDto : UpdateDto
{
    public string? MethodName { get; set; }
    public string? MethodCode { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Method { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
}