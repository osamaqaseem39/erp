using ERP.Domain;

namespace ERP.Domain.Sales;

public class POSTransactionLine : BaseEntity
{
    public Guid POSTransactionId { get; set; }
    public Guid ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int LineNumber { get; set; }
}