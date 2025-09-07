using ERP.Domain;

namespace ERP.Domain.SCM;

public class BatchNumber : BaseEntity
{
    public Guid ItemId { get; set; }
    public string BatchNumberValue { get; set; } = string.Empty;
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; } = 0;
    public new bool IsActive { get; set; } = true;
    public new Guid TenantId { get; set; }
    
    // Navigation properties
    public Item Item { get; set; } = null!;
}
