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
        TaxRate = line.Tax1Rate,
        TaxAmount = line.Tax1Amount + line.Tax2Amount,
        Tax1Rate = line.Tax1Rate,
        Tax1Amount = line.Tax1Amount,
        Tax2Rate = line.Tax2Rate,
        Tax2Amount = line.Tax2Amount,
        TotalPrice = line.LineTotal,
        Notes = line.Notes
    };

    public static SalesOrderLine ToEntity(CreateSalesOrderLineDto dto, Guid salesOrderId, Guid tenantId)
    {
        var discount = dto.DiscountPercent ?? 0;
        var net = CrmLineCalculator.CalculateLineTotal(dto.Quantity, dto.UnitPrice, discount);
        var tax1Rate = dto.Tax1Rate ?? dto.TaxRate ?? 0;
        var tax2Rate = dto.Tax2Rate ?? 0;
        var taxes = CrmLineCalculator.CalculateLineTaxes(net, tax1Rate, tax2Rate);
        return new SalesOrderLine
        {
            SalesOrderId = salesOrderId,
            TenantId = tenantId,
            ItemId = dto.ItemId == Guid.Empty ? null : dto.ItemId,
            ItemName = dto.Description ?? string.Empty,
            Description = dto.Description,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            DiscountPercent = discount,
            Tax1Rate = tax1Rate,
            Tax1Amount = taxes.Tax1Amount,
            Tax2Rate = tax2Rate,
            Tax2Amount = taxes.Tax2Amount,
            LineTotal = net,
            Notes = dto.Notes
        };
    }
}
