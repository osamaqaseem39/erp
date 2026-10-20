using ERP.Domain.Sales;
using ERP.Shared.DTOs.Sales;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Sales;

public static class SalesInvoiceMapper
{
    public static SalesInvoiceDto ToDto(SalesInvoice entity)
    {
        return new SalesInvoiceDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            InvoiceNumber = entity.InvoiceNumber,
            CustomerId = entity.CustomerId,
            CustomerName = null, // populated by SalesInvoiceService from Customers
            InvoiceDate = entity.InvoiceDate,
            DueDate = entity.DueDate,
            SubTotal = entity.SubTotal,
            TaxAmount = entity.TaxAmount,
            TotalAmount = entity.TotalAmount,
            Status = entity.Status,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static SalesInvoice ToEntity(CreateSalesInvoiceDto dto, Guid tenantId, string createdBy)
    {
        return new SalesInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvoiceNumber = dto.InvoiceNumber,
            CustomerId = dto.CustomerId,
            InvoiceDate = dto.InvoiceDate,
            DueDate = dto.DueDate,
            SubTotal = dto.SubTotal,
            TaxAmount = dto.TaxAmount,
            TotalAmount = dto.TotalAmount,
            Status = dto.Status ?? "Draft",
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateSalesInvoiceDto dto, SalesInvoice entity, string updatedBy)
    {
        entity.InvoiceNumber = dto.InvoiceNumber;
        entity.CustomerId = dto.CustomerId;
        entity.InvoiceDate = dto.InvoiceDate;
        entity.DueDate = dto.DueDate;
        entity.SubTotal = dto.SubTotal;
        entity.TaxAmount = dto.TaxAmount;
        entity.TotalAmount = dto.TotalAmount;
        entity.Status = dto.Status;
        entity.Notes = dto.Notes;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}