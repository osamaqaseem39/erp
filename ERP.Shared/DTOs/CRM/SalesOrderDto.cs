using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.CRM;

public class SalesOrderDto : BaseDto
{
    public Guid CompanyId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string SalesOrderNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Guid? QuoteId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? InvoicedDate { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal SubTotal { get; set; }
    public decimal Tax1Amount { get; set; }
    public decimal Tax2Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Currency { get; set; }
    public string? Terms { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? Notes { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public int CreditDays { get; set; }
    public string? PaymentMode { get; set; }
    public Guid? PaymentTermId { get; set; }
    public decimal Freight { get; set; }
    public decimal Overhead { get; set; }
    public string? DiscountType { get; set; }
    public decimal DiscountAmount { get; set; }
    public new bool IsActive { get; set; } = true;
    
    // Navigation properties
    public string? CustomerName { get; set; }
    public string? QuoteNumber { get; set; }
    public string? AssignedToUserName { get; set; }
    public List<SalesOrderLineDto> SalesOrderLines { get; set; } = new();
}

public class CreateSalesOrderDto : CreateDto
{
    public Guid CompanyId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string SalesOrderNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Guid? QuoteId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Currency { get; set; }
    public string? Terms { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? Notes { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public int CreditDays { get; set; }
    public string? PaymentMode { get; set; }
    public Guid? PaymentTermId { get; set; }
    public decimal Freight { get; set; }
    public decimal Overhead { get; set; }
    public string? DiscountType { get; set; }
    public decimal DiscountAmount { get; set; }
    public List<CreateSalesOrderLineDto> SalesOrderLines { get; set; } = new();
}

public class UpdateSalesOrderDto : UpdateDto
{
    public string? OrderNumber { get; set; }
    public string? SalesOrderNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? QuoteId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public string? Status { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; }
    public string? Terms { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? Notes { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public int? CreditDays { get; set; }
    public string? PaymentMode { get; set; }
    public Guid? PaymentTermId { get; set; }
    public decimal? Freight { get; set; }
    public decimal? Overhead { get; set; }
    public string? DiscountType { get; set; }
    public decimal? DiscountAmount { get; set; }
    public List<CreateSalesOrderLineDto> SalesOrderLines { get; set; } = new();
}