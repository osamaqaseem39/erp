using ERP.Domain;

namespace ERP.Domain.SCM;

public class UnitOfMeasure : BaseEntity
{
    public string UnitCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UnitType { get; set; } = "Base"; // Base, Weight, Length, Volume, etc.
    public decimal? ConversionFactor { get; set; }
    public Guid? BaseUnitId { get; set; }
    public new bool IsActive { get; set; } = true;
    public new Guid TenantId { get; set; }
    
    // Navigation properties
    public UnitOfMeasure? BaseUnit { get; set; }
    public ICollection<UnitOfMeasure> DerivedUnits { get; set; } = new List<UnitOfMeasure>();
}
