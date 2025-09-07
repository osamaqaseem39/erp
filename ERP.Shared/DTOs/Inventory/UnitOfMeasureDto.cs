using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class UnitOfMeasureDto : BaseDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal ConversionFactor { get; set; }
    public Guid? BaseUnitId { get; set; }
    public string? BaseUnitName { get; set; }
    public bool IsBaseUnit { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUnitOfMeasureDto : CreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal ConversionFactor { get; set; }
    public Guid? BaseUnitId { get; set; }
    public bool IsBaseUnit { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUnitOfMeasureDto : UpdateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal ConversionFactor { get; set; }
    public Guid? BaseUnitId { get; set; }
    public bool IsBaseUnit { get; set; }
    public bool IsActive { get; set; }
}