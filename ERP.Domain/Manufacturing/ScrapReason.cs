using ERP.Domain;

namespace ERP.Domain.Manufacturing;

public class ScrapReason : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
