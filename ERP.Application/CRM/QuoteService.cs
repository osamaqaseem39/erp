using ERP.Application.Interfaces.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

public class QuoteService : IQuoteService
{
    private readonly IQuoteRepository _repository;
    private readonly IQuoteLineRepository _lineRepository;
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly ISalesOrderLineRepository _salesOrderLineRepository;

    public QuoteService(
        IQuoteRepository repository,
        IQuoteLineRepository lineRepository,
        ISalesOrderRepository salesOrderRepository,
        ISalesOrderLineRepository salesOrderLineRepository)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _salesOrderRepository = salesOrderRepository;
        _salesOrderLineRepository = salesOrderLineRepository;
    }

    public async Task<QuoteDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<QuoteDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateQuoteDto dto)
    {
        var entity = MapToEntity(dto);
        if (string.IsNullOrWhiteSpace(entity.QuoteNumber))
            entity.QuoteNumber = CrmLineCalculator.GenerateNumber("QT");

        ApplyLineTotals(entity, dto.QuoteLines);
        var quoteId = await _repository.CreateAsync(entity);
        await SaveLinesAsync(quoteId, entity.TenantId, dto.QuoteLines);
        return quoteId;
    }

    public async Task<bool> UpdateAsync(UpdateQuoteDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        ApplyLineTotals(entity, dto.QuoteLines);
        await _lineRepository.DeleteByQuoteIdAsync(entity.Id);
        await SaveLinesAsync(entity.Id, entity.TenantId, dto.QuoteLines);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _lineRepository.DeleteByQuoteIdAsync(id);
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<QuoteDto>> GetByCustomerAsync(Guid customerId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<QuoteDto>> GetByStatusAsync(string status)
    {
        var entities = await _repository.GetByStatusAsync(status);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<QuoteDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate);
        return entities.Select(MapToDto);
    }

    public async Task<bool> SendQuoteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = "Sent";
        entity.SentDate = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<Guid> ConvertToSalesOrderAsync(Guid id)
    {
        var quote = await _repository.GetByIdAsync(id);
        if (quote == null) return Guid.Empty;

        var lines = quote.QuoteLines.Any()
            ? quote.QuoteLines
            : (await _lineRepository.GetByQuoteIdAsync(id)).ToList();

        var orderNumber = CrmLineCalculator.GenerateNumber("SO");
        var salesOrder = new SalesOrder
        {
            TenantId = quote.TenantId,
            CompanyId = quote.CompanyId ?? Guid.Empty,
            OrderNumber = orderNumber,
            SalesOrderNumber = orderNumber,
            CustomerId = quote.CustomerId,
            QuoteId = quote.Id,
            OrderDate = DateTime.UtcNow,
            Status = "Draft",
            SubTotal = quote.SubTotal,
            TaxAmount = quote.TaxAmount,
            TotalAmount = quote.TotalAmount,
            Currency = quote.Currency,
            Notes = quote.Notes,
            Terms = quote.Terms,
            AssignedToUserId = quote.AssignedToUserId,
            CreatedBy = quote.CreatedBy
        };

        var salesOrderId = await _salesOrderRepository.CreateAsync(salesOrder);

        foreach (var line in lines)
        {
            await _salesOrderLineRepository.CreateAsync(new SalesOrderLine
            {
                TenantId = quote.TenantId,
                SalesOrderId = salesOrderId,
                ItemId = line.ItemId,
                ItemName = line.ItemName,
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                DiscountPercent = line.DiscountPercent,
                LineTotal = line.LineTotal,
                Notes = line.Notes
            });
        }

        quote.Status = "Converted";
        quote.ConvertedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(quote);

        return salesOrderId;
    }

    private async Task SaveLinesAsync(Guid quoteId, Guid tenantId, IEnumerable<CreateQuoteLineDto> lines)
    {
        foreach (var lineDto in lines)
        {
            var line = CrmLineMapper.ToEntity(lineDto, quoteId, tenantId);
            await _lineRepository.CreateAsync(line);
        }
    }

    private static void ApplyLineTotals(Quote entity, IEnumerable<CreateQuoteLineDto> lines)
    {
        var lineList = lines.ToList();
        if (lineList.Count == 0) return;

        var totals = CrmLineCalculator.CalculateTotals(lineList.Select(l =>
            (CrmLineCalculator.CalculateLineTotal(l.Quantity, l.UnitPrice, l.DiscountPercent ?? 0), l.TaxRate ?? 0)));

        entity.SubTotal = totals.SubTotal;
        entity.TaxAmount = totals.TaxAmount;
        entity.TotalAmount = totals.TotalAmount;
    }

    private static QuoteDto MapToDto(Quote entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        CompanyId = entity.CompanyId,
        QuoteNumber = entity.QuoteNumber,
        CustomerId = entity.CustomerId,
        OpportunityId = entity.OpportunityId,
        CustomerName = entity.Customer?.CompanyName ?? string.Empty,
        QuoteDate = entity.QuoteDate,
        ValidUntil = entity.ValidUntil,
        Status = entity.Status,
        SubTotal = entity.SubTotal,
        TaxAmount = entity.TaxAmount,
        TotalAmount = entity.TotalAmount,
        Currency = entity.Currency,
        Notes = entity.Notes,
        Terms = entity.Terms,
        SentDate = entity.SentDate,
        ConvertedDate = entity.ConvertedDate,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy,
        IsActive = entity.IsActive,
        QuoteLines = entity.QuoteLines.Select(CrmLineMapper.ToDto).ToList()
    };

    private static Quote MapToEntity(CreateQuoteDto dto) => new()
    {
        TenantId = dto.TenantId,
        CompanyId = dto.CompanyId,
        QuoteNumber = dto.QuoteNumber ?? string.Empty,
        CustomerId = dto.CustomerId,
        OpportunityId = dto.OpportunityId,
        QuoteDate = dto.QuoteDate ?? DateTime.UtcNow,
        ValidUntil = dto.ValidUntil ?? DateTime.UtcNow.AddDays(30),
        ExpiryDate = dto.ValidUntil ?? DateTime.UtcNow.AddDays(30),
        Status = dto.Status ?? "Draft",
        SubTotal = dto.SubTotal ?? 0m,
        TaxAmount = dto.TaxAmount ?? 0m,
        TotalAmount = dto.TotalAmount ?? 0m,
        Currency = dto.Currency ?? "USD",
        Notes = dto.Notes,
        Terms = dto.Terms,
        AssignedToUserId = dto.AssignedToUserId,
        CreatedBy = dto.CreatedBy
    };

    private static void MapToEntity(UpdateQuoteDto dto, Quote entity)
    {
        entity.CustomerId = dto.CustomerId;
        entity.OpportunityId = dto.OpportunityId;
        entity.QuoteDate = dto.QuoteDate ?? entity.QuoteDate;
        entity.ValidUntil = dto.ValidUntil ?? entity.ValidUntil;
        entity.ExpiryDate = dto.ValidUntil ?? entity.ExpiryDate;
        entity.Status = dto.Status ?? entity.Status;
        entity.SubTotal = dto.SubTotal ?? entity.SubTotal;
        entity.TaxAmount = dto.TaxAmount ?? entity.TaxAmount;
        entity.TotalAmount = dto.TotalAmount ?? entity.TotalAmount;
        entity.Currency = dto.Currency ?? entity.Currency;
        entity.Notes = dto.Notes;
        entity.Terms = dto.Terms;
        entity.AssignedToUserId = dto.AssignedToUserId;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
