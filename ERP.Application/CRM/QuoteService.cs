using ERP.Application.Common;
using ERP.Application.Interfaces.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Infrastructure.Numbering;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

public class QuoteService : IQuoteService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "Sent", "Accepted", "Rejected" },
            ["Sent"] = new[] { "Accepted", "Rejected" },
            ["Accepted"] = new[] { "Converted" },
            ["Rejected"] = Array.Empty<string>(),
            ["Converted"] = Array.Empty<string>(),
            ["Expired"] = Array.Empty<string>()
        };

    private readonly IDocumentNumberService _numbers;
    private readonly IQuoteRepository _repository;
    private readonly IQuoteLineRepository _lineRepository;
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly ISalesOrderLineRepository _salesOrderLineRepository;

    public QuoteService(
        IQuoteRepository repository,
        IQuoteLineRepository lineRepository,
        ISalesOrderRepository salesOrderRepository,
        ISalesOrderLineRepository salesOrderLineRepository,
        IDocumentNumberService numbers)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _salesOrderRepository = salesOrderRepository;
        _salesOrderLineRepository = salesOrderLineRepository;
        _numbers = numbers;
    }

    public async Task<QuoteDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<QuoteDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateQuoteDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var entity = MapToEntity(dto);
        entity.TenantId = tenantId;
        // ERP-FIX-5: always start Draft
        entity.Status = "Draft";
        entity.CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy;
        if (string.IsNullOrWhiteSpace(entity.QuoteNumber))
            entity.QuoteNumber = await _numbers.NextAsync("crm.quote");

        ApplyLineTotals(entity, dto.QuoteLines);
        var quoteId = await _repository.CreateAsync(entity);
        await SaveLinesAsync(quoteId, tenantId, dto.QuoteLines);
        return quoteId;
    }

    public async Task<bool> UpdateAsync(UpdateQuoteDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity);
        // ERP-FIX-5: status only via send/approve/reject/convert
        entity.Status = previousStatus;
        entity.UpdatedBy = userId != Guid.Empty ? userId.ToString() : dto.UpdatedBy;
        ApplyLineTotals(entity, dto.QuoteLines);
        await _lineRepository.DeleteByQuoteIdAsync(entity.Id, tenantId);
        await SaveLinesAsync(entity.Id, tenantId, dto.QuoteLines);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        await _lineRepository.DeleteByQuoteIdAsync(id, tenantId);
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<QuoteDto>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByCustomerAsync(customerId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<QuoteDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByStatusAsync(status, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<QuoteDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<bool> SendQuoteAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Sent", Transitions))
            return false;

        entity.Status = "Sent";
        entity.SentDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Accepted", Transitions))
            return false;
        // ERP-FIX-6: creator cannot final-approve
        if (DocumentWorkflow.IsSameActor(entity.CreatedBy, userId))
            return false;

        entity.Status = "Accepted";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Rejected", Transitions))
            return false;

        entity.Status = "Rejected";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<Guid> ConvertToSalesOrderAsync(Guid id, Guid tenantId, Guid userId)
    {
        var quote = await GetScopedAsync(id, tenantId);
        if (quote == null) return Guid.Empty;
        if (!DocumentWorkflow.CanTransition(quote.Status, "Converted", Transitions))
            return Guid.Empty;

        var lines = quote.QuoteLines.Any()
            ? quote.QuoteLines
            : (await _lineRepository.GetByQuoteIdAsync(id, tenantId)).ToList();

        var orderNumber = await _numbers.NextAsync("crm.salesOrder");
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
            CreatedBy = userId == Guid.Empty ? quote.CreatedBy : userId.ToString()
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
        quote.UpdatedBy = userId.ToString();
        quote.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(quote);

        return salesOrderId;
    }

    private async Task<Quote?> GetScopedAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _repository.GetByIdAsync(id, tenantId);
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
        entity.TotalAmount = CrmLineCalculator.ApplyDocumentTotal(
            totals.TotalAmount, entity.Freight, entity.Overhead, entity.DiscountType, entity.DiscountAmount);
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
        ReferenceNo = entity.ReferenceNo,
        ReferenceDate = entity.ReferenceDate,
        CreditDays = entity.CreditDays,
        PaymentMode = entity.PaymentMode,
        PaymentTermId = entity.PaymentTermId,
        Freight = entity.Freight,
        Overhead = entity.Overhead,
        DiscountType = entity.DiscountType,
        DiscountAmount = entity.DiscountAmount,
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
        Status = "Draft",
        SubTotal = dto.SubTotal ?? 0m,
        TaxAmount = dto.TaxAmount ?? 0m,
        TotalAmount = dto.TotalAmount ?? 0m,
        Currency = dto.Currency ?? "USD",
        Notes = dto.Notes,
        Terms = dto.Terms,
        AssignedToUserId = dto.AssignedToUserId,
        ReferenceNo = dto.ReferenceNo,
        ReferenceDate = dto.ReferenceDate,
        CreditDays = dto.CreditDays,
        PaymentMode = dto.PaymentMode,
        PaymentTermId = dto.PaymentTermId,
        Freight = dto.Freight,
        Overhead = dto.Overhead,
        DiscountType = dto.DiscountType,
        DiscountAmount = dto.DiscountAmount,
        CreatedBy = dto.CreatedBy
    };

    private static void MapToEntity(UpdateQuoteDto dto, Quote entity)
    {
        entity.CustomerId = dto.CustomerId;
        entity.OpportunityId = dto.OpportunityId;
        entity.QuoteDate = dto.QuoteDate ?? entity.QuoteDate;
        entity.ValidUntil = dto.ValidUntil ?? entity.ValidUntil;
        entity.ExpiryDate = dto.ValidUntil ?? entity.ExpiryDate;
        entity.SubTotal = dto.SubTotal ?? entity.SubTotal;
        entity.TaxAmount = dto.TaxAmount ?? entity.TaxAmount;
        entity.TotalAmount = dto.TotalAmount ?? entity.TotalAmount;
        entity.Currency = dto.Currency ?? entity.Currency;
        entity.Notes = dto.Notes;
        entity.Terms = dto.Terms;
        entity.AssignedToUserId = dto.AssignedToUserId;
        entity.ReferenceNo = dto.ReferenceNo;
        entity.ReferenceDate = dto.ReferenceDate;
        if (dto.CreditDays.HasValue) entity.CreditDays = dto.CreditDays.Value;
        entity.PaymentMode = dto.PaymentMode;
        entity.PaymentTermId = dto.PaymentTermId;
        if (dto.Freight.HasValue) entity.Freight = dto.Freight.Value;
        if (dto.Overhead.HasValue) entity.Overhead = dto.Overhead.Value;
        entity.DiscountType = dto.DiscountType;
        if (dto.DiscountAmount.HasValue) entity.DiscountAmount = dto.DiscountAmount.Value;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
