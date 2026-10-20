using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class InvoiceDto : BaseDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Balance { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? SalesOrderId { get; set; }
    public string? SalesOrderNumber { get; set; }
    public Guid? SourceSalesInvoiceId { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public bool IsLocked { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ProjectMilestoneId { get; set; }
    public List<InvoiceLineDto>? InvoiceLines { get; set; }
}

public class CreateInvoiceDto : CreateDto
{
    public string? InvoiceNumber { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? CurrencyCode { get; set; } = "USD";
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Status { get; set; } = "Draft";
    public Guid? SalesOrderId { get; set; }
    public Guid? SourceSalesInvoiceId { get; set; }
    public bool? IsLocked { get; set; } = false;
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ProjectMilestoneId { get; set; }
}

public class InvoiceLineDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? TaxAmount { get; set; }
    public Guid? GLAccountId { get; set; }
    public string? GLAccountCode { get; set; }
    public string? GLAccountName { get; set; }
    public string? Notes { get; set; }
}

public class UpdateInvoiceDto : UpdateDto
{
    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? Status { get; set; }
    public Guid? SalesOrderId { get; set; }
    public bool? IsLocked { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
}