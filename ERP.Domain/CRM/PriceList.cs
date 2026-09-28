using ERP.Domain;

namespace ERP.Domain.CRM;

public class PriceList : BaseEntity
{
    public string PriceListName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsDefault { get; set; }
}
