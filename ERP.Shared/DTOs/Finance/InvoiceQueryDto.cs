using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class InvoiceQuery
{
    public Guid TenantId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}


public class InvoiceLineQueryDto : BaseDto
{
    public Guid InvoiceLineId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
}

public class InvoiceLineCreateDto : CreateDto
{
    public Guid InvoiceId { get; set; }
    public Guid ItemId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public Guid GLAccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
}

public class InvoiceLineUpdateDto : UpdateDto
{
    public Guid? ItemId { get; set; }
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TaxRate { get; set; }
    public Guid? GLAccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
}

public class BillLineDto : BaseDto
{
    public Guid BillLineId { get; set; }
    public Guid BillId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
}

public class BillLineCreateDto : CreateDto
{
    public Guid BillId { get; set; }
    public Guid ItemId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public Guid GLAccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
}

public class BillLineUpdateDto : UpdateDto
{
    public Guid? ItemId { get; set; }
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TaxRate { get; set; }
    public Guid? GLAccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }
}


// Aliases for interface compatibility
public class InvoiceCreateDto : CreateInvoiceDto { }
public class InvoiceUpdateDto : UpdateInvoiceDto { }
public class BillCreateDto : CreateBillDto { }
public class BillUpdateDto : UpdateBillDto { }