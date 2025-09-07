using ERP.Domain;

namespace ERP.Domain.Sales;

public class POSTransaction : BaseEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public Guid POSSessionId { get; set; }
    public Guid SessionId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeAmount { get; set; }
    public string Status { get; set; } = "Completed";
    public string? Notes { get; set; }
}