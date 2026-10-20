using ERP.Domain;

namespace ERP.Domain.SCM;

public class WarehouseLocation : BaseEntity
{
    public Guid WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    /// <summary>Pick | Bulk | Staging | QC</summary>
    public string LocationType { get; set; } = "Pick";
    public string? Zone { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
}
