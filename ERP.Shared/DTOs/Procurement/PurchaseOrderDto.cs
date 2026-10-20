using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Procurement;

public class PurchaseOrderDto : BaseDto
{
    public Guid? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorCode { get; set; }
    public Guid? RFQId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? Currency { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DeliveryTerms { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public string? ApprovedBy { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public List<PurchaseOrderDetailDto>? Details { get; set; }
    public List<PurchaseOrderDetailDto>? PurchaseOrderDetails { get; set; }
}

public class CreatePurchaseOrderDto : CreateDto
{
    public Guid? CompanyId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public Guid VendorId { get; set; }
    public Guid? RFQId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Currency { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DeliveryTerms { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public List<CreatePurchaseOrderDetailDto> Details { get; set; } = new();
}

public class UpdatePurchaseOrderDto : UpdateDto
{
    public Guid? PurchaseOrderId { get; set; }
    public Guid? CompanyId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? RFQId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? Currency { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DeliveryTerms { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public List<CreatePurchaseOrderDetailDto> Details { get; set; } = new();
}

public class PurchaseOrderDetailDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid POId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? Discount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? ReceivedQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? Notes { get; set; }
}

public class CreatePurchaseOrderDetailDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePurchaseOrderDetailDto
{
    public Guid? ItemId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Notes { get; set; }
}

// Aliases for GraphQL compatibility
public class PurchaseOrderCreateDto : CreatePurchaseOrderDto { }
public class PurchaseOrderUpdateDto : UpdatePurchaseOrderDto { }