using ERP.Domain;

namespace ERP.Domain.CRM;

public class SalesTerritory : BaseEntity
{
    public string TerritoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
}
