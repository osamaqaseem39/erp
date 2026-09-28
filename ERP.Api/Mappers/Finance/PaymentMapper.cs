using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Mappers.Finance;

public class PaymentMapper : BaseMapper<Payment, PaymentDto>
{
    public override PaymentDto Map(Payment source)
    {
        if (source == null) return null!;

        return new PaymentDto
        {
            Id = source.Id,
            PaymentId = source.Id,
            PaymentNumber = source.PaymentNumber,
            PaymentDate = source.PaymentDate,
            PaymentType = source.PaymentType,
            PaymentMethod = source.PaymentMethod,
            Amount = source.Amount,
            Currency = source.Currency,
            CurrencyCode = source.CurrencyCode,
            BankAccountId = source.BankAccountId,
            RelatedInvoiceId = source.RelatedInvoiceId,
            RelatedBillId = source.RelatedBillId,
            Status = source.Status,
            Reference = source.Reference,
            ReferenceNumber = source.ReferenceNumber,
            Notes = source.Notes,
            ProcessedDate = source.ProcessedDate,
            ProcessedAt = source.ProcessedAt,
            ProcessedBy = source.ProcessedBy,
            CompanyId = source.CompanyId,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override Payment Map(PaymentDto destination)
    {
        if (destination == null) return null!;

        return new Payment
        {
            Id = destination.Id,
            PaymentNumber = destination.PaymentNumber ?? string.Empty,
            PaymentDate = destination.PaymentDate ?? DateTime.UtcNow,
            PaymentType = destination.PaymentType ?? string.Empty,
            PaymentMethod = destination.PaymentMethod ?? string.Empty,
            Amount = destination.Amount ?? 0,
            Currency = destination.Currency ?? "USD",
            CurrencyCode = destination.CurrencyCode ?? "USD",
            BankAccountId = destination.BankAccountId,
            RelatedInvoiceId = destination.RelatedInvoiceId,
            RelatedBillId = destination.RelatedBillId,
            Status = destination.Status ?? "Pending",
            Reference = destination.Reference,
            ReferenceNumber = destination.ReferenceNumber,
            Notes = destination.Notes,
            ProcessedDate = destination.ProcessedDate,
            ProcessedAt = destination.ProcessedAt,
            ProcessedBy = destination.ProcessedBy,
            CompanyId = destination.CompanyId,
            TenantId = destination.TenantId != Guid.Empty ? destination.TenantId : Guid.Empty,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

