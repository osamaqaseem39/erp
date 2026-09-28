using ERP.Domain;

namespace ERP.Domain.CRM;

public class CustomerGroup : BaseEntity
{
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercentage { get; set; }
}
