namespace ERP.Shared.DTOs.Reports;

public class PendingSalesOrderLineDto
{
    public Guid SalesOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class PendingSalesOrderReportDto
{
    public DateTime? AsOfDate { get; set; }
    public List<PendingSalesOrderLineDto> Lines { get; set; } = new();
    public int OrderCount { get; set; }
    public decimal TotalValue { get; set; }
}

public class SalesOrderScheduleLineDto
{
    public Guid SalesOrderId { get; set; }
    public Guid? SalesOrderLineId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SalesOrderScheduleReportDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<SalesOrderScheduleLineDto> Lines { get; set; } = new();
    public int LineCount { get; set; }
}

public class SalesInvoiceSummaryLineDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
}

public class SalesInvoiceSummaryReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<SalesInvoiceSummaryLineDto> Lines { get; set; } = new();
    public int InvoiceCount { get; set; }
    public decimal TotalSubTotal { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalBalance { get; set; }
}

public class SalesSummaryByItemLineDto
{
    public Guid ItemId { get; set; }
    public string? ItemDescription { get; set; }
    public decimal Quantity { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int InvoiceCount { get; set; }
}

public class SalesSummaryByItemReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<SalesSummaryByItemLineDto> Lines { get; set; } = new();
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
}

public class SalesCustomerItemLineDto
{
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemDescription { get; set; }
    public decimal Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class SalesCustomerItemReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<SalesCustomerItemLineDto> Lines { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class SalesItemRateDetailLineDto
{
    public Guid ItemId { get; set; }
    public string? ItemDescription { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal Tax1Rate { get; set; }
    public decimal Tax2Rate { get; set; }
}

public class SalesItemRateDetailReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? CustomerId { get; set; }
    public List<SalesItemRateDetailLineDto> Lines { get; set; } = new();
}

public class CustomerInfoReportLineDto
{
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public int CreditDays { get; set; }
    public string? TaxType { get; set; }
    public string? Ntn { get; set; }
    public string? Strn { get; set; }
    public string? Cnic { get; set; }
    public Guid? SalesmanId { get; set; }
    public string? SalesmanName { get; set; }
}

public class CustomerInfoReportDto
{
    public List<CustomerInfoReportLineDto> Lines { get; set; } = new();
    public int CustomerCount { get; set; }
}

public class CreditNoteReportLineDto
{
    public Guid CreditNoteId { get; set; }
    public string CreditNoteNumber { get; set; } = string.Empty;
    public DateTime CreditNoteDate { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid OriginalSalesInvoiceId { get; set; }
}

public class CreditNoteReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<CreditNoteReportLineDto> Lines { get; set; } = new();
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

public class PurchaseInvoiceSummaryLineDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public Guid VendorId { get; set; }
    public string? VendorName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetPayable { get; set; }
}

public class PurchaseInvoiceSummaryReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<PurchaseInvoiceSummaryLineDto> Lines { get; set; } = new();
    public int InvoiceCount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class StockBalanceReportLineDto
{
    public Guid ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
}

public class StockBalanceReportDto
{
    public Guid? WarehouseId { get; set; }
    public List<StockBalanceReportLineDto> Lines { get; set; } = new();
    public decimal TotalQuantity { get; set; }
}
