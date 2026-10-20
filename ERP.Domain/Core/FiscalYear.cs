using ERP.Domain;

namespace ERP.Domain.Core;

/// <summary>Tenant fiscal period. Status: Active | Closed.</summary>
public class FiscalYear : BaseEntity
{
    public Guid FiscalYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    /// <summary>Active | Closed</summary>
    public string Status { get; set; } = "Active";
}
