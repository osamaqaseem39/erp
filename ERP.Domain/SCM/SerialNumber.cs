using ERP.Domain;

namespace ERP.Domain.SCM;

public class SerialNumber : BaseEntity
{
    public Guid ItemId { get; set; }
    public string SerialNumberValue { get; set; } = string.Empty;
    public string Status { get; set; } = "Available"; // Available, Sold, Reserved, Damaged
    public string? Location { get; set; }
    public new bool IsActive { get; set; } = true;
    public new Guid TenantId { get; set; }
    
    // Navigation properties
    public Item Item { get; set; } = null!;
}
