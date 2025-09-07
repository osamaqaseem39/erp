using ERP.Domain.Sales;
using ERP.Shared.DTOs.Sales;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Sales;

public static class POSTransactionMapper
{
    public static POSTransactionDto ToDto(POSTransaction entity)
    {
        return new POSTransactionDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            TransactionNumber = entity.TransactionNumber,
            SessionId = entity.SessionId,
            CustomerId = entity.CustomerId,
            TransactionDate = entity.TransactionDate,
            SubTotal = entity.SubTotal,
            TaxAmount = entity.TaxAmount,
            TotalAmount = entity.TotalAmount,
            PaymentMethod = entity.PaymentMethod,
            Status = entity.Status,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static POSTransaction ToEntity(CreatePOSTransactionDto dto, Guid tenantId, string createdBy)
    {
        return new POSTransaction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransactionNumber = dto.TransactionNumber,
            SessionId = dto.SessionId,
            CustomerId = dto.CustomerId,
            TransactionDate = dto.TransactionDate,
            SubTotal = dto.SubTotal,
            TaxAmount = dto.TaxAmount,
            TotalAmount = dto.TotalAmount,
            PaymentMethod = dto.PaymentMethod,
            Status = dto.Status ?? "Completed",
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdatePOSTransactionDto dto, POSTransaction entity, string updatedBy)
    {
        entity.TransactionNumber = dto.TransactionNumber;
        entity.SessionId = dto.SessionId;
        entity.CustomerId = dto.CustomerId;
        entity.TransactionDate = dto.TransactionDate;
        entity.SubTotal = dto.SubTotal;
        entity.TaxAmount = dto.TaxAmount;
        entity.TotalAmount = dto.TotalAmount;
        entity.PaymentMethod = dto.PaymentMethod;
        entity.Status = dto.Status;
        entity.Notes = dto.Notes;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}