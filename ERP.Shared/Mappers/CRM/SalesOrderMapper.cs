using ERP.Domain.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Shared.Mappers.CRM;

public static class SalesOrderMapper
{
    public static SalesOrderDto ToDto(this SalesOrder entity)
    {
        return new SalesOrderDto
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            CustomerId = entity.CustomerId,
            QuoteId = entity.QuoteId,
            OrderDate = entity.OrderDate,
            RequiredDate = entity.RequiredDate,
            ShippedDate = entity.ShippedDate,
            Status = entity.Status,
            SubTotal = entity.SubTotal,
            TaxAmount = entity.TaxAmount,
            TotalAmount = entity.TotalAmount,
            ShippingAddress = entity.ShippingAddress,
            BillingAddress = entity.BillingAddress,
            Notes = entity.Notes,
            AssignedToUserId = entity.AssignedToUserId,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CustomerName = entity.Customer?.CustomerName,
            QuoteNumber = entity.Quote?.QuoteNumber,
            AssignedToUserName = entity.AssignedToUser?.Username
        };
    }

    public static SalesOrder ToEntity(this SalesOrderDto dto)
    {
        return new SalesOrder
        {
            Id = dto.Id,
            OrderNumber = dto.OrderNumber,
            CustomerId = dto.CustomerId,
            QuoteId = dto.QuoteId,
            OrderDate = dto.OrderDate,
            RequiredDate = dto.RequiredDate,
            ShippedDate = dto.ShippedDate,
            Status = dto.Status,
            SubTotal = dto.SubTotal,
            TaxAmount = dto.TaxAmount,
            TotalAmount = dto.TotalAmount,
            ShippingAddress = dto.ShippingAddress,
            BillingAddress = dto.BillingAddress,
            Notes = dto.Notes,
            AssignedToUserId = dto.AssignedToUserId,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}