using ERP.Domain;

namespace ERP.Domain.SCM;

public class Brand : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
