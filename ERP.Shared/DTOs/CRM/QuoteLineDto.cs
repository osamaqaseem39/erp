using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.CRM;

public class QuoteLineDto
{
    public Guid Id { get; set; }
    public Guid QuoteId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
}

public class CreateQuoteLineDto
{
    public Guid ItemId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? TaxRate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateQuoteLineDto
{
    public Guid? ItemId { get; set; }
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? TaxRate { get; set; }
    public string? Notes { get; set; }
}