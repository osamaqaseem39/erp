using ERP.Domain;

namespace ERP.Domain.CRM;

public class QuoteLine : BaseEntity
{
    public Guid QuoteId { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Quote Quote { get; set; } = null!;
}