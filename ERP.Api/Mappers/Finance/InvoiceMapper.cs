using ERP.Domain.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Mappers.Finance;

public class InvoiceMapper : BaseMapper<Invoice, InvoiceDto>
{
    public override InvoiceDto Map(Invoice source)
    {
        if (source == null) return null!;

        return new InvoiceDto
        {
            Id = source.Id,
            InvoiceId = source.InvoiceId,
            InvoiceNumber = source.InvoiceNumber,
            CustomerId = source.CustomerId,
            InvoiceDate = source.InvoiceDate,
            DueDate = source.DueDate,
            CurrencyCode = source.CurrencyCode,
            SubTotal = source.SubTotal,
            TaxTotal = source.TaxTotal,
            TaxAmount = source.TaxTotal,
            TotalAmount = source.TotalAmount,
            Balance = source.Balance,
            BalanceAmount = source.Balance,
            PaidAmount = source.TotalAmount - source.Balance,
            Status = source.Status,
            SalesOrderId = source.SalesOrderId,
            PostedAt = source.PostedAt,
            PostedBy = source.PostedBy,
            IsLocked = source.IsLocked,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override Invoice Map(InvoiceDto destination)
    {
        if (destination == null) return null!;

        return new Invoice
        {
            Id = destination.Id,
            InvoiceId = destination.InvoiceId,
            InvoiceNumber = destination.InvoiceNumber,
            CustomerId = destination.CustomerId,
            InvoiceDate = destination.InvoiceDate,
            DueDate = destination.DueDate,
            CurrencyCode = destination.CurrencyCode,
            SubTotal = destination.SubTotal,
            TaxTotal = destination.TaxTotal,
            TotalAmount = destination.TotalAmount,
            Balance = destination.Balance,
            Status = destination.Status,
            SalesOrderId = destination.SalesOrderId,
            PostedAt = destination.PostedAt,
            PostedBy = destination.PostedBy,
            IsLocked = destination.IsLocked,
            TenantId = destination.TenantId,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

