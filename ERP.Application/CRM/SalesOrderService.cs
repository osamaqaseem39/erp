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

    public async Task<SalesOrderDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<SalesOrderDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateSalesOrderDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var entity = MapToEntity(dto);
        entity.TenantId = tenantId;
        // ERP-FIX-5: always start Draft
        entity.Status = "Draft";
        entity.CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy;
        if (string.IsNullOrWhiteSpace(entity.OrderNumber))
        {
            entity.OrderNumber = await _numbers.NextAsync("crm.salesOrder");
            entity.SalesOrderNumber = entity.OrderNumber;
        }

        ApplyLineTotals(entity, dto.SalesOrderLines);
        var orderId = await _repository.CreateAsync(entity);
        await SaveLinesAsync(orderId, tenantId, dto.SalesOrderLines);
        return orderId;
    }

    public async Task<bool> UpdateAsync(UpdateSalesOrderDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity);
        // ERP-FIX-5: status only via approve/reject/ship/invoice
        entity.Status = previousStatus;
        entity.UpdatedBy = userId != Guid.Empty ? userId.ToString() : dto.UpdatedBy;
        ApplyLineTotals(entity, dto.SalesOrderLines);
        await _lineRepository.DeleteBySalesOrderIdAsync(entity.Id, tenantId);
        await SaveLinesAsync(entity.Id, tenantId, dto.SalesOrderLines);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        await _lineRepository.DeleteBySalesOrderIdAsync(id, tenantId);
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByCustomerAsync(customerId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByStatusAsync(status, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate, tenantId);
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
            : (await _lineRepository.GetBySalesOrderIdAsync(id, tenantId)).ToList();

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
        return await _repository.GetByIdAsync(id, tenantId);
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
        entity.TotalAmount = CrmLineCalculator.ApplyDocumentTotal(
            totals.TotalAmount, entity.Freight, entity.Overhead, entity.DiscountType, entity.DiscountAmount);
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
        ReferenceNo = entity.ReferenceNo,
        ReferenceDate = entity.ReferenceDate,
        CreditDays = entity.CreditDays,
        PaymentMode = entity.PaymentMode,
        PaymentTermId = entity.PaymentTermId,
        Freight = entity.Freight,
        Overhead = entity.Overhead,
        DiscountType = entity.DiscountType,
        DiscountAmount = entity.DiscountAmount,
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
        entity.ReferenceNo = dto.ReferenceNo ?? entity.ReferenceNo;
        entity.ReferenceDate = dto.ReferenceDate ?? entity.ReferenceDate;
        if (dto.CreditDays.HasValue) entity.CreditDays = dto.CreditDays.Value;
        entity.PaymentMode = dto.PaymentMode ?? entity.PaymentMode;
        entity.PaymentTermId = dto.PaymentTermId ?? entity.PaymentTermId;
        if (dto.Freight.HasValue) entity.Freight = dto.Freight.Value;
        if (dto.Overhead.HasValue) entity.Overhead = dto.Overhead.Value;
        entity.DiscountType = dto.DiscountType ?? entity.DiscountType;
        if (dto.DiscountAmount.HasValue) entity.DiscountAmount = dto.DiscountAmount.Value;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
