using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Procurement;

public class PurchaseDebitNoteDto : BaseDto
{
    public Guid? Id { get; set; }
    public string DebitNoteNumber { get; set; } = string.Empty;
    public DateTime DebitNoteDate { get; set; }
    public Guid VendorId { get; set; }
    public Guid OriginalPurchaseInvoiceId { get; set; }
    public Guid? PurchaseReturnId { get; set; }
    public Guid? WarehouseId { get; set; }
    public bool StockIssued { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Reason { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public Guid? AppliedFinanceBillId { get; set; }
    public Guid? PostedJournalId { get; set; }
    public List<PurchaseDebitNoteLineDto> Lines { get; set; } = new();
}

public class CreatePurchaseDebitNoteDto : CreateDto
{
    public string DebitNoteNumber { get; set; } = string.Empty;
    public DateTime DebitNoteDate { get; set; }
    public Guid VendorId { get; set; }
    public Guid OriginalPurchaseInvoiceId { get; set; }
    public Guid? PurchaseReturnId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseDebitNoteLineDto> Lines { get; set; } = new();
}

public class UpdatePurchaseDebitNoteDto : UpdateDto
{
    public string DebitNoteNumber { get; set; } = string.Empty;
    public DateTime DebitNoteDate { get; set; }
    public Guid VendorId { get; set; }
    public Guid OriginalPurchaseInvoiceId { get; set; }
    public Guid? PurchaseReturnId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public List<UpdatePurchaseDebitNoteLineDto> Lines { get; set; } = new();
}

public class VoidPurchaseDebitNoteDto
{
    public string? Reason { get; set; }
}

public class PurchaseDebitNoteLineDto
{
    public Guid Id { get; set; }
    public Guid DebitNoteId { get; set; }
    public int LineNumber { get; set; }
    public Guid? OriginalPurchaseInvoiceLineId { get; set; }
    public Guid? ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreatePurchaseDebitNoteLineDto
{
    public Guid? OriginalPurchaseInvoiceLineId { get; set; }
    public Guid? ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int LineNumber { get; set; }
}

public class UpdatePurchaseDebitNoteLineDto
{
    public Guid Id { get; set; }
    public Guid? OriginalPurchaseInvoiceLineId { get; set; }
    public Guid? ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int LineNumber { get; set; }
}
