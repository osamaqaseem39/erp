using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Finance;

public class PaymentDto : BaseDto
{
    public Guid? PaymentId { get; set; }
    public string? PaymentNumber { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentType { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? CurrencyCode { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? BankAccountName { get; set; }
    public Guid? RelatedInvoiceId { get; set; }
    public string? RelatedInvoiceNumber { get; set; }
    public Guid? RelatedBillId { get; set; }
    public string? RelatedBillNumber { get; set; }
    public string? Status { get; set; }
    public string? Reference { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedBy { get; set; }
    public new bool? IsActive { get; set; } = true;
    public Guid? CompanyId { get; set; }
}

public class PaymentQuery
{
    public Guid TenantId { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class CreatePaymentDto : CreateDto
{
    public string? PaymentNumber { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentType { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? CurrencyCode { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid? RelatedInvoiceId { get; set; }
    public Guid? RelatedBillId { get; set; }
    public string? Status { get; set; } = "Pending";
    public string? Reference { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public Guid? CompanyId { get; set; }
}

public class UpdatePaymentDto : UpdateDto
{
    public Guid? PaymentId { get; set; }
    public string? PaymentNumber { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentType { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? CurrencyCode { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid? RelatedInvoiceId { get; set; }
    public Guid? RelatedBillId { get; set; }
    public string? Status { get; set; }
    public string? Reference { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}