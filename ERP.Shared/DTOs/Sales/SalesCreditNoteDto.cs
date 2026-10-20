using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Sales;

public class SalesCreditNoteDto : BaseDto
{
    public Guid? Id { get; set; }
    public string CreditNoteNumber { get; set; } = string.Empty;
    public DateTime CreditNoteDate { get; set; }
    public Guid CustomerId { get; set; }
    public Guid OriginalSalesInvoiceId { get; set; }
    public Guid? ReturnOrderId { get; set; }
    public Guid? WarehouseId { get; set; }
    public bool StockReceived { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Reason { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public Guid? AppliedFinanceInvoiceId { get; set; }
    public Guid? PostedJournalId { get; set; }
    public List<SalesCreditNoteLineDto> Lines { get; set; } = new();
}

public class CreateSalesCreditNoteDto : CreateDto
{
    public string CreditNoteNumber { get; set; } = string.Empty;
    public DateTime CreditNoteDate { get; set; }
    public Guid CustomerId { get; set; }
    public Guid OriginalSalesInvoiceId { get; set; }
    public Guid? ReturnOrderId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public List<CreateSalesCreditNoteLineDto> Lines { get; set; } = new();
}

public class UpdateSalesCreditNoteDto : UpdateDto
{
    public string CreditNoteNumber { get; set; } = string.Empty;
    public DateTime CreditNoteDate { get; set; }
    public Guid CustomerId { get; set; }
    public Guid OriginalSalesInvoiceId { get; set; }
    public Guid? ReturnOrderId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public List<UpdateSalesCreditNoteLineDto> Lines { get; set; } = new();
}

public class VoidSalesCreditNoteDto
{
    public string? Reason { get; set; }
}

public class SalesCreditNoteLineDto
{
    public Guid Id { get; set; }
    public Guid CreditNoteId { get; set; }
    public int LineNumber { get; set; }
    public Guid? OriginalSalesInvoiceLineId { get; set; }
    public Guid? ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreateSalesCreditNoteLineDto
{
    public Guid? OriginalSalesInvoiceLineId { get; set; }
    public Guid? ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int LineNumber { get; set; }
}

public class UpdateSalesCreditNoteLineDto
{
    public Guid Id { get; set; }
    public Guid? OriginalSalesInvoiceLineId { get; set; }
    public Guid? ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int LineNumber { get; set; }
}
