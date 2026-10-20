namespace ERP.Domain.Core;

public class PaymentTerm : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Days { get; set; }
    public string? Description { get; set; }
}
