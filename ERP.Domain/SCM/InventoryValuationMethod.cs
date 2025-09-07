using ERP.Domain;

namespace ERP.Domain.SCM;

public class InventoryValuationMethod : BaseEntity
{
    public string MethodName { get; set; } = string.Empty;
    public string MethodCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public new bool IsActive { get; set; } = true;
    public new Guid TenantId { get; set; }
}
