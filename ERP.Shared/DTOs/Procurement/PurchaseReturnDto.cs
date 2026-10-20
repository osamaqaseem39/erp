namespace ERP.Shared.DTOs.Procurement;

public class PurchaseReturnLineDto
{
    public Guid Id { get; set; }
    public Guid PurchaseReturnId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Reason { get; set; }
}

public class CreatePurchaseReturnLineDto
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Reason { get; set; }
}

public class PurchaseReturnDto : BaseDto
{
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public Guid VendorId { get; set; }
    public string? VendorName { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? PurchaseInvoiceId { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? DebitNoteId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseReturnLineDto> Lines { get; set; } = new();
}

public class CreatePurchaseReturnDto : CreateDto
{
    public string? ReturnNumber { get; set; }
    public DateTime ReturnDate { get; set; }
    public Guid VendorId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? PurchaseInvoiceId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseReturnLineDto> Lines { get; set; } = new();
}

public class UpdatePurchaseReturnDto : UpdateDto
{
    public string? Reason { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Notes { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? PurchaseInvoiceId { get; set; }
    public List<CreatePurchaseReturnLineDto>? Lines { get; set; }
}
