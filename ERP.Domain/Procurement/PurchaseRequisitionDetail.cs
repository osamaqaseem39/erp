using ERP.Domain;

namespace ERP.Domain.Procurement;

public class PurchaseRequisitionDetail : BaseEntity
{
    public Guid PRId { get; set; }
    public Guid ItemId { get; set; }
    public decimal QuantityRequested { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal EstimatedUnitPrice { get; set; } = 0;
    public string Remarks { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual PurchaseRequisition PurchaseRequisition { get; set; } = null!;
    public virtual SCM.Item Item { get; set; } = null!;
    public virtual SCM.UnitOfMeasure UnitOfMeasure { get; set; } = null!;
}
