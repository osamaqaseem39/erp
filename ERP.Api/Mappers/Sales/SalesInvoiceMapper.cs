using ERP.Domain.Sales;
using ERP.Shared.DTOs.Sales;

namespace ERP.Api.Mappers.Sales;

public class SalesInvoiceMapper : BaseMapper<SalesInvoice, SalesInvoiceDto>
{
    public override SalesInvoiceDto Map(SalesInvoice source)
    {
        if (source == null) return null!;

        return new SalesInvoiceDto
        {
            Id = source.Id,
            InvoiceNumber = source.InvoiceNumber,
            CustomerId = source.CustomerId,
            CustomerName = null, // populated by SalesInvoiceService
            InvoiceDate = source.InvoiceDate,
            DueDate = source.DueDate,
            Status = source.Status,
            SubTotal = source.SubTotal,
            TaxAmount = source.TaxAmount,
            TotalAmount = source.TotalAmount,
            PaidAmount = source.PaidAmount,
            BalanceAmount = source.BalanceAmount,
            Notes = source.Notes,
            Terms = source.Terms,
            SalesOrderId = source.SalesOrderId,
            PostedAt = source.PostedAt,
            PostedBy = source.PostedBy,
            TenantId = source.TenantId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    public override SalesInvoice Map(SalesInvoiceDto destination)
    {
        if (destination == null) return null!;

        return new SalesInvoice
        {
            Id = destination.Id.HasValue ? destination.Id.Value : Guid.NewGuid(),
            InvoiceNumber = destination.InvoiceNumber,
            CustomerId = destination.CustomerId,
            InvoiceDate = destination.InvoiceDate,
            DueDate = destination.DueDate,
            Status = destination.Status,
            SubTotal = destination.SubTotal,
            TaxAmount = destination.TaxAmount,
            TotalAmount = destination.TotalAmount,
            PaidAmount = destination.PaidAmount,
            BalanceAmount = destination.BalanceAmount,
            Notes = destination.Notes,
            Terms = destination.Terms,
            SalesOrderId = destination.SalesOrderId,
            PostedAt = destination.PostedAt,
            PostedBy = destination.PostedBy,
            TenantId = destination.TenantId,
            CreatedAt = destination.CreatedAt,
            UpdatedAt = destination.UpdatedAt
        };
    }
}

