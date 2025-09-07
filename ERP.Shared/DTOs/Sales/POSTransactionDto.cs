using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Sales;

public class POSTransactionDto : BaseDto
{
    public Guid? Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public Guid POSSessionId { get; set; }
    public Guid SessionId { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime TransactionDate { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeAmount { get; set; }
    public string Status { get; set; } = "Completed";
    public string? Notes { get; set; }
    public List<POSTransactionLineDto> Lines { get; set; } = new();
}

public class CreatePOSTransactionDto : CreateDto
{
    public string TransactionNumber { get; set; } = string.Empty;
    public Guid POSSessionId { get; set; }
    public Guid SessionId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public string Status { get; set; } = "Completed";
    public string? Notes { get; set; }
    public List<CreatePOSTransactionLineDto> Lines { get; set; } = new();
}

public class UpdatePOSTransactionDto : UpdateDto
{
    public string TransactionNumber { get; set; } = string.Empty;
    public Guid SessionId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public string Status { get; set; } = "Completed";
    public string? Notes { get; set; }
    public List<UpdatePOSTransactionLineDto> Lines { get; set; } = new();
}

public class POSTransactionLineDto
{
    public new Guid Id { get; set; }
    public Guid POSTransactionId { get; set; }
    public Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int LineNumber { get; set; }
}

public class CreatePOSTransactionLineDto
{
    public Guid ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal TaxRate { get; set; }
    public int LineNumber { get; set; }
}

public class UpdatePOSTransactionLineDto
{
    public new Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal TaxRate { get; set; }
    public int LineNumber { get; set; }
}