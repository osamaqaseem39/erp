using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Finance;

public static class PaymentMapper
{
    public static PaymentDto ToDto(Payment entity)
    {
        return new PaymentDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            PaymentNumber = entity.PaymentNumber,
            PaymentDate = entity.PaymentDate,
            PaymentType = entity.PaymentType,
            PaymentMethod = entity.PaymentMethod,
            Amount = entity.Amount,
            Currency = entity.Currency,
            CurrencyCode = entity.CurrencyCode,
            BankAccountId = entity.BankAccountId,
            BankAccountName = entity.BankAccount?.AccountName,
            RelatedInvoiceId = entity.RelatedInvoiceId,
            RelatedInvoiceNumber = entity.RelatedInvoice?.InvoiceNumber,
            RelatedBillId = entity.RelatedBillId,
            RelatedBillNumber = entity.RelatedBill?.BillNumber,
            Status = entity.Status,
            Reference = entity.Reference,
            ReferenceNumber = entity.ReferenceNumber,
            Notes = entity.Notes,
            ProcessedDate = entity.ProcessedDate,
            ProcessedAt = entity.ProcessedAt,
            ProcessedBy = entity.ProcessedBy,
            IsActive = entity.IsActive,
            CompanyId = entity.CompanyId,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static Payment ToEntity(CreatePaymentDto dto, Guid tenantId, string createdBy)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PaymentNumber = dto.PaymentNumber ?? string.Empty,
            PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
            PaymentType = dto.PaymentType ?? string.Empty,
            PaymentMethod = dto.PaymentMethod ?? string.Empty,
            Amount = dto.Amount ?? 0m,
            Currency = dto.Currency ?? "USD",
            CurrencyCode = dto.CurrencyCode ?? "USD",
            BankAccountId = dto.BankAccountId,
            RelatedInvoiceId = dto.RelatedInvoiceId,
            RelatedBillId = dto.RelatedBillId,
            Status = dto.Status ?? "Pending",
            Reference = dto.Reference,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            CompanyId = dto.CompanyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdatePaymentDto dto, Payment entity, string updatedBy)
    {
        if (!string.IsNullOrEmpty(dto.PaymentNumber))
            entity.PaymentNumber = dto.PaymentNumber;
        
        if (dto.PaymentDate.HasValue)
            entity.PaymentDate = dto.PaymentDate.Value;
        
        if (!string.IsNullOrEmpty(dto.PaymentType))
            entity.PaymentType = dto.PaymentType;
        
        if (!string.IsNullOrEmpty(dto.PaymentMethod))
            entity.PaymentMethod = dto.PaymentMethod;
        
        if (dto.Amount.HasValue)
            entity.Amount = dto.Amount.Value;
        
        if (!string.IsNullOrEmpty(dto.Currency))
            entity.Currency = dto.Currency;
        
        if (!string.IsNullOrEmpty(dto.CurrencyCode))
            entity.CurrencyCode = dto.CurrencyCode;
        
        if (dto.BankAccountId.HasValue)
            entity.BankAccountId = dto.BankAccountId;
        
        if (dto.RelatedInvoiceId.HasValue)
            entity.RelatedInvoiceId = dto.RelatedInvoiceId;
        
        if (dto.RelatedBillId.HasValue)
            entity.RelatedBillId = dto.RelatedBillId;
        
        // Status only via payment workflow actions
        
        entity.Reference = dto.Reference;
        entity.ReferenceNumber = dto.ReferenceNumber;
        entity.Notes = dto.Notes;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}