using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.CRM;

public class SalesOrderLineDto
{
    public Guid Id { get; set; }
    public Guid SalesOrderId { get; set; }
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

public class CreateSalesOrderLineDto
{
    public Guid ItemId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? TaxRate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSalesOrderLineDto
{
    public Guid? ItemId { get; set; }
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? TaxRate { get; set; }
    public string? Notes { get; set; }
}