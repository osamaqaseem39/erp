using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Sales;

public class SalesInvoiceDto : BaseDto
{
    public Guid? Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public Guid? SalesOrderId { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public List<SalesInvoiceLineDto> Lines { get; set; } = new();
}

public class CreateSalesInvoiceDto : CreateDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public Guid? SalesOrderId { get; set; }
    public List<CreateSalesInvoiceLineDto> Lines { get; set; } = new();
}

public class UpdateSalesInvoiceDto : UpdateDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public List<UpdateSalesInvoiceLineDto> Lines { get; set; } = new();
}

public class SalesInvoiceLineDto
{
    public new Guid Id { get; set; }
    public Guid SalesInvoiceId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int LineNumber { get; set; }
}

public class CreateSalesInvoiceLineDto
{
    public Guid ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int LineNumber { get; set; }
}

public class UpdateSalesInvoiceLineDto
{
    public new Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int LineNumber { get; set; }
}