using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Procurement;

public class PurchaseInvoiceDto : BaseDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorCode { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public Guid? POId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public decimal NetPayable { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public string? Currency { get; set; } = "USD";
    public List<PurchaseInvoiceDetailDto>? Details { get; set; }
    public List<PurchaseInvoiceDetailDto>? InvoiceDetails { get; set; }
}

public class CreatePurchaseInvoiceDto : CreateDto
{
    public string? InvoiceNumber { get; set; }
    public Guid VendorId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public string? Terms { get; set; }
}

public class UpdatePurchaseInvoiceDto : UpdateDto
{
    public string? InvoiceNumber { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
}

public class PurchaseInvoiceDetailDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class CreatePurchaseInvoiceDetailDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? TaxRate { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePurchaseInvoiceDetailDto
{
    public Guid? ItemId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TaxRate { get; set; }
    public string? Notes { get; set; }
}

// Aliases for GraphQL compatibility
public class PurchaseInvoiceCreateDto : CreatePurchaseInvoiceDto { }
public class PurchaseInvoiceUpdateDto : UpdatePurchaseInvoiceDto { }