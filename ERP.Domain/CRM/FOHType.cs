using ERP.Domain;

namespace ERP.Domain.CRM;

public class FOHType : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Basis { get; set; }
    public decimal Rate { get; set; }
    public string? Description { get; set; }
}
