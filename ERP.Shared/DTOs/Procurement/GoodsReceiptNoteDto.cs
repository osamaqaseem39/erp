using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Procurement;

public class GoodsReceiptNoteDto : BaseDto
{
    public Guid? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string GRNNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorCode { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? POId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public DateTime ReceiptDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid? ReceivedByUserId { get; set; }
    public string? ReceivedByName { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Remarks { get; set; }
    public string? Notes { get; set; }
    public string? ReceivedBy { get; set; }
    public string? InspectedBy { get; set; }
    public List<GoodsReceiptNoteDetailDto>? Details { get; set; }
    public List<GoodsReceiptNoteDetailDto>? GRNDetails { get; set; }
}

public class CreateGoodsReceiptNoteDto : CreateDto
{
    public Guid? CompanyId { get; set; }
    public string? GRNNumber { get; set; }
    public Guid VendorId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? POId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public string? Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public string? Remarks { get; set; }
    public string? ReceivedBy { get; set; }
    public string? InspectedBy { get; set; }
    public List<CreateGoodsReceiptNoteDetailDto> Details { get; set; } = new();
}

public class UpdateGoodsReceiptNoteDto : UpdateDto
{
    public Guid? CompanyId { get; set; }
    public string? GRNNumber { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? POId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public string? Remarks { get; set; }
    public string? ReceivedBy { get; set; }
    public string? InspectedBy { get; set; }
    public List<CreateGoodsReceiptNoteDetailDto> Details { get; set; } = new();
}

public class GoodsReceiptNoteDetailDto
{
    public Guid Id { get; set; }
    public Guid GRNId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public Guid? BatchSerialId { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class CreateGoodsReceiptNoteDetailDto
{
    public Guid ItemId { get; set; }
    public decimal? OrderedQuantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }
    public decimal? AcceptedQuantity { get; set; }
    public decimal? RejectedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
}

public class UpdateGoodsReceiptNoteDetailDto
{
    public decimal? OrderedQuantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }
    public decimal? AcceptedQuantity { get; set; }
    public decimal? RejectedQuantity { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
}

// Aliases for GraphQL compatibility
public class GoodsReceiptNoteCreateDto : CreateGoodsReceiptNoteDto { }
public class GoodsReceiptNoteUpdateDto : UpdateGoodsReceiptNoteDto { }