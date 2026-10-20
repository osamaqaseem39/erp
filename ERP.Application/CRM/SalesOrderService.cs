using ERP.Application.Common;
using ERP.Infrastructure.Numbering;
using ERP.Application.Interfaces.CRM;
using ERP.Application.Interfaces.Finance;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Shared.DTOs.Finance;
using IStockService = ERP.Application.Interfaces.IStockService;

namespace ERP.Application.CRM;

public class SalesOrderService : ISalesOrderService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "Approved", "Cancelled" },
            ["Approved"] = new[] { "Shipped", "Cancelled" },
            ["Shipped"] = new[] { "Invoiced" },
            ["Invoiced"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IDocumentNumberService _numbers;
    private readonly ISalesOrderRepository _repository;
    private readonly ISalesOrderLineRepository _lineRepository;
    private readonly IInvoiceService _invoiceService;
    private readonly IStockService _stockService;

    public SalesOrderService(
        ISalesOrderRepository repository,
        ISalesOrderLineRepository lineRepository,
        IInvoiceService invoiceService,
        IDocumentNumberService numbers,
        IStockService stockService)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _invoiceService = invoiceService;
        _numbers = numbers;
        _stockService = stockService;
    }

    public async Task<SalesOrderDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<SalesOrderDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateSalesOrderDto dto)
    {
        var entity = MapToEntity(dto);
        // ERP-FIX-5: always start Draft
        entity.Status = "Draft";
        if (string.IsNullOrWhiteSpace(entity.OrderNumber))
        {
            entity.OrderNumber = await _numbers.NextAsync("crm.salesOrder");
            entity.SalesOrderNumber = entity.OrderNumber;
        }

        ApplyLineTotals(entity, dto.SalesOrderLines);
        var orderId = await _repository.CreateAsync(entity);
        await SaveLinesAsync(orderId, entity.TenantId, dto.SalesOrderLines);
        return orderId;
    }

    public async Task<bool> UpdateAsync(UpdateSalesOrderDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity);
        // ERP-FIX-5: status only via approve/reject/ship/invoice
        entity.Status = previousStatus;
        ApplyLineTotals(entity, dto.SalesOrderLines);
        await _lineRepository.DeleteBySalesOrderIdAsync(entity.Id);
        await SaveLinesAsync(entity.Id, entity.TenantId, dto.SalesOrderLines);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        await _lineRepository.DeleteBySalesOrderIdAsync(id);
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByCustomerAsync(Guid customerId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByStatusAsync(string status)
    {
        var entities = await _repository.GetByStatusAsync(status);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate);
        return entities.Select(MapToDto);
    }

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Approved", Transitions))
            return false;
        // ERP-FIX-6: creator cannot final-approve
        if (DocumentWorkflow.IsSameActor(entity.CreatedBy, userId))
            return false;

        entity.Status = "Approved";
        entity.ApprovedDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Cancelled", Transitions))
            return false;

        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> ShipAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Shipped", Transitions))
            return false;

        var lines = entity.SalesOrderLines?.Count > 0
            ? entity.SalesOrderLines
            : (await _lineRepository.GetBySalesOrderIdAsync(id)).ToList();

        var stockLines = lines
            .Where(l => l.ItemId.HasValue && l.ItemId != Guid.Empty && l.Quantity > 0)
            .Select(l => (l.ItemId!.Value, l.Quantity))
            .ToList();

        if (stockLines.Count > 0)
        {
            var issued = await _stockService.IssueStockAnyWarehouseAsync(
                entity.TenantId, stockLines, userId.ToString());
            if (!issued) return false;
        }

        entity.Status = "Shipped";
        entity.ShippedDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> InvoiceAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;

        if (string.Equals(DocumentWorkflow.Normalize(entity.Status), "Invoiced", StringComparison.OrdinalIgnoreCase))
            return true;

        if (!DocumentWorkflow.CanTransition(entity.Status, "Invoiced", Transitions))
            return false;

        if (entity.CustomerId == null || entity.CustomerId == Guid.Empty)
            return false;

        var invoiceDto = new InvoiceCreateDto
        {
            CustomerId = entity.CustomerId.Value,
            InvoiceNumber = $"INV-{entity.OrderNumber ?? entity.Id.ToString("N")[..8]}",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            CurrencyCode = "USD",
            SubTotal = entity.SubTotal,
            TaxAmount = entity.TaxAmount,
            TotalAmount = entity.TotalAmount,
            Status = "Draft",
            SalesOrderId = entity.Id
        };

        await _invoiceService.AddInvoiceAsync(invoiceDto, entity.TenantId, userId);

        entity.Status = "Invoiced";
        entity.InvoicedDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    private async Task<SalesOrder?> GetScopedAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId) return null;
        return entity;
    }

    private async Task SaveLinesAsync(Guid salesOrderId, Guid tenantId, IEnumerable<CreateSalesOrderLineDto> lines)
    {
        foreach (var lineDto in lines)
        {
            var line = CrmLineMapper.ToEntity(lineDto, salesOrderId, tenantId);
            await _lineRepository.CreateAsync(line);
        }
    }

    private static void ApplyLineTotals(SalesOrder entity, IEnumerable<CreateSalesOrderLineDto> lines)
    {
        var lineList = lines.ToList();
        if (lineList.Count == 0) return;

        var totals = CrmLineCalculator.CalculateTotals(lineList.Select(l =>
        (
            CrmLineCalculator.CalculateLineTotal(l.Quantity, l.UnitPrice, l.DiscountPercent ?? 0),
            l.Tax1Rate ?? l.TaxRate ?? 0,
            l.Tax2Rate ?? 0
        )));

        entity.SubTotal = totals.SubTotal;
        entity.Tax1Amount = totals.Tax1Amount;
        entity.Tax2Amount = totals.Tax2Amount;
        entity.TaxAmount = totals.TaxAmount;
        entity.TotalAmount = totals.TotalAmount;
    }

    private static SalesOrderDto MapToDto(SalesOrder entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        CompanyId = entity.CompanyId,
        OrderNumber = entity.OrderNumber,
        SalesOrderNumber = entity.SalesOrderNumber,
        CustomerId = entity.CustomerId,
        QuoteId = entity.QuoteId,
        CustomerName = entity.Customer?.CompanyName ?? string.Empty,
        OrderDate = entity.OrderDate,
        RequiredDate = entity.RequiredDate,
        Status = entity.Status,
        SubTotal = entity.SubTotal,
        Tax1Amount = entity.Tax1Amount,
        Tax2Amount = entity.Tax2Amount,
        TaxAmount = entity.TaxAmount,
        TotalAmount = entity.TotalAmount,
        Currency = entity.Currency,
        Notes = entity.Notes,
        Terms = entity.Terms,
        ApprovedDate = entity.ApprovedDate,
        ShippedDate = entity.ShippedDate,
        InvoicedDate = entity.InvoicedDate,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy,
        IsActive = entity.IsActive,
        SalesOrderLines = entity.SalesOrderLines.Select(CrmLineMapper.ToDto).ToList()
    };

    private static SalesOrder MapToEntity(CreateSalesOrderDto dto) => new()
    {
        TenantId = dto.TenantId,
        CompanyId = dto.CompanyId,
        OrderNumber = dto.OrderNumber,
        SalesOrderNumber = string.IsNullOrWhiteSpace(dto.SalesOrderNumber) ? dto.OrderNumber : dto.SalesOrderNumber,
        CustomerId = dto.CustomerId,
        QuoteId = dto.QuoteId,
        OrderDate = dto.OrderDate,
        RequiredDate = dto.RequiredDate,
        Status = "Draft",
        SubTotal = dto.SubTotal,
        TaxAmount = dto.TaxAmount,
        TotalAmount = dto.TotalAmount,
        Currency = dto.Currency,
        Notes = dto.Notes,
        Terms = dto.Terms,
        CreatedBy = dto.CreatedBy
    };

    private static void MapToEntity(UpdateSalesOrderDto dto, SalesOrder entity)
    {
        if (dto.CustomerId.HasValue) entity.CustomerId = dto.CustomerId;
        if (dto.QuoteId.HasValue) entity.QuoteId = dto.QuoteId;
        if (dto.OrderDate.HasValue) entity.OrderDate = dto.OrderDate.Value;
        if (dto.RequiredDate.HasValue) entity.RequiredDate = dto.RequiredDate;
        if (dto.SubTotal.HasValue) entity.SubTotal = dto.SubTotal.Value;
        if (dto.TaxAmount.HasValue) entity.TaxAmount = dto.TaxAmount.Value;
        if (dto.TotalAmount.HasValue) entity.TotalAmount = dto.TotalAmount.Value;
        if (!string.IsNullOrEmpty(dto.Currency)) entity.Currency = dto.Currency;
        entity.Notes = dto.Notes ?? entity.Notes;
        entity.Terms = dto.Terms ?? entity.Terms;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
