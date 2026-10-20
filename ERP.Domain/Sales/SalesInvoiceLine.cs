using ERP.Domain;

namespace ERP.Domain.Sales;

public class SalesInvoiceLine : BaseEntity
{
    public Guid SalesInvoiceId { get; set; }
    public Guid ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    /// <summary>Legacy alias for Tax1Rate.</summary>
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Tax1Rate { get; set; }
    public decimal Tax1Amount { get; set; }
    public decimal Tax2Rate { get; set; }
    public decimal Tax2Amount { get; set; }
    public decimal LineTotal { get; set; }
    public int LineNumber { get; set; }
}