using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class PaymentApplicationDto : BaseDto
{
    public Guid PaymentId { get; set; }
    public string? PaymentNumber { get; set; }
    public Guid InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public Guid BillId { get; set; }
    public string? BillNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime ApplicationDate { get; set; }
    public string Status { get; set; } = "Applied";
    public string? Notes { get; set; }
}

public class CreatePaymentApplicationDto : CreateDto
{
    public Guid PaymentId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? ApplicationDate { get; set; }
    public string? Status { get; set; } = "Applied";
    public string? Notes { get; set; }
}

public class UpdatePaymentApplicationDto : UpdateDto
{
    public decimal? Amount { get; set; }
    public DateTime? ApplicationDate { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}