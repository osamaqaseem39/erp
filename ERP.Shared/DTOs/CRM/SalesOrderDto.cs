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
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Currency { get; set; }
    public string? Terms { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? Notes { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public new bool IsActive { get; set; } = true;
    
    // Navigation properties
    public string? CustomerName { get; set; }
    public string? QuoteNumber { get; set; }
    public string? AssignedToUserName { get; set; }
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
}