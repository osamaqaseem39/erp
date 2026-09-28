using ERP.Domain.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

public static class CrmLineMapper
{
    public static QuoteLineDto ToDto(QuoteLine line) => new()
    {
        Id = line.Id,
        QuoteId = line.QuoteId,
        ItemId = line.ItemId ?? Guid.Empty,
        ItemName = line.ItemName,
        Description = line.Description,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        DiscountPercent = line.DiscountPercent,
        TotalPrice = line.LineTotal,
        Notes = line.Notes
    };

    public static QuoteLine ToEntity(CreateQuoteLineDto dto, Guid quoteId, Guid tenantId) => new()
    {
        QuoteId = quoteId,
        TenantId = tenantId,
        ItemId = dto.ItemId == Guid.Empty ? null : dto.ItemId,
        ItemName = dto.Description ?? string.Empty,
        Description = dto.Description,
        Quantity = dto.Quantity,
        UnitPrice = dto.UnitPrice,
        DiscountPercent = dto.DiscountPercent ?? 0,
        LineTotal = CrmLineCalculator.CalculateLineTotal(dto.Quantity, dto.UnitPrice, dto.DiscountPercent ?? 0)
    };

    public static SalesOrderLineDto ToDto(SalesOrderLine line) => new()
    {
        Id = line.Id,
        SalesOrderId = line.SalesOrderId,
        ItemId = line.ItemId ?? Guid.Empty,
        ItemName = line.ItemName,
        Description = line.Description,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        DiscountPercent = line.DiscountPercent,
        TotalPrice = line.LineTotal,
        Notes = line.Notes
    };

    public static SalesOrderLine ToEntity(CreateSalesOrderLineDto dto, Guid salesOrderId, Guid tenantId) => new()
    {
        SalesOrderId = salesOrderId,
        TenantId = tenantId,
        ItemId = dto.ItemId == Guid.Empty ? null : dto.ItemId,
        ItemName = dto.Description ?? string.Empty,
        Description = dto.Description,
        Quantity = dto.Quantity,
        UnitPrice = dto.UnitPrice,
        DiscountPercent = dto.DiscountPercent ?? 0,
        LineTotal = CrmLineCalculator.CalculateLineTotal(dto.Quantity, dto.UnitPrice, dto.DiscountPercent ?? 0)
    };
}
