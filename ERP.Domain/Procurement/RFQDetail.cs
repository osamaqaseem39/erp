using ERP.Domain;

namespace ERP.Domain.Procurement;

public class RFQDetail : BaseEntity
{
    public Guid RFQId { get; set; }
    public Guid ItemId { get; set; }
    public decimal QuantityRequested { get; set; }
    public Guid? UnitOfMeasureId { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string? Specifications { get; set; }
    public new string CreatedBy { get; set; } = string.Empty;

    // Navigation properties
    public virtual RequestForQuotation RequestForQuotation { get; set; } = null!;
    public virtual SCM.Item? Item { get; set; }
    public virtual SCM.UnitOfMeasure? UnitOfMeasure { get; set; }
}
