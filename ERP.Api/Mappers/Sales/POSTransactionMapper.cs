using ERP.Domain.Sales;
using ERP.Shared.DTOs.Sales;

namespace ERP.Api.Mappers.Sales;

public class POSTransactionMapper : BaseMapper<POSTransaction, POSTransactionDto>
{
    public override POSTransactionDto Map(POSTransaction source)
    {
        if (source == null) return null!;

        return new POSTransactionDto
        {
            Id = source.Id,
            TransactionNumber = source.TransactionNumber,
            POSSessionId = source.POSSessionId,
            SessionId = source.SessionId,
            CustomerId = source.CustomerId,
            TransactionDate = source.TransactionDate,
            PaymentMethod = source.PaymentMethod,
            SubTotal = source.SubTotal,
            TaxAmount = source.TaxAmount,
            DiscountAmount = source.DiscountAmount,
            TotalAmount = source.TotalAmount,
            AmountPaid = source.AmountPaid,
            ChangeAmount = source.ChangeAmount,
            Status = source.Status,
            Notes = source.Notes,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override POSTransaction Map(POSTransactionDto destination)
    {
        if (destination == null) return null!;

        return new POSTransaction
        {
            Id = destination.Id.HasValue ? destination.Id.Value : Guid.NewGuid(),
            TransactionNumber = destination.TransactionNumber,
            POSSessionId = destination.POSSessionId,
            SessionId = destination.SessionId,
            CustomerId = destination.CustomerId,
            TransactionDate = destination.TransactionDate,
            PaymentMethod = destination.PaymentMethod,
            SubTotal = destination.SubTotal,
            TaxAmount = destination.TaxAmount,
            DiscountAmount = destination.DiscountAmount,
            TotalAmount = destination.TotalAmount,
            AmountPaid = destination.AmountPaid,
            ChangeAmount = destination.ChangeAmount,
            Status = destination.Status,
            Notes = destination.Notes,
            TenantId = destination.TenantId,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

