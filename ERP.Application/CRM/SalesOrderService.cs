using ERP.Application.Interfaces.CRM;
using ERP.Application.Interfaces.Finance;
using ERP.Domain.CRM;
using ERP.Domain.Finance;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Shared.DTOs.Finance;

namespace ERP.Application.CRM;

public class SalesOrderService : ISalesOrderService
{
    private readonly ISalesOrderRepository _repository;
    private readonly ISalesOrderLineRepository _lineRepository;
    private readonly IInvoiceService _invoiceService;

    public SalesOrderService(
        ISalesOrderRepository repository,
        ISalesOrderLineRepository lineRepository,
        IInvoiceService invoiceService)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _invoiceService = invoiceService;
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
        if (string.IsNullOrWhiteSpace(entity.OrderNumber))
        {
            entity.OrderNumber = CrmLineCalculator.GenerateNumber("SO");
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

        MapToEntity(dto, entity);
        ApplyLineTotals(entity, dto.SalesOrderLines);
        await _lineRepository.DeleteBySalesOrderIdAsync(entity.Id);
        await SaveLinesAsync(entity.Id, entity.TenantId, dto.SalesOrderLines);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
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

    public async Task<bool> ApproveAsync(Guid id) => await UpdateStatusAsync(id, "Approved", o => o.ApprovedDate = DateTime.UtcNow);

    public async Task<bool> ShipAsync(Guid id) => await UpdateStatusAsync(id, "Shipped", o => o.ShippedDate = DateTime.UtcNow);

    public async Task<bool> InvoiceAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;
        if (string.Equals(entity.Status, "Invoiced", StringComparison.OrdinalIgnoreCase))
            return true;

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

        await _invoiceService.AddInvoiceAsync(invoiceDto, entity.TenantId, Guid.Empty);

        entity.Status = "Invoiced";
        entity.InvoicedDate = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    private async Task<bool> UpdateStatusAsync(Guid id, string status, Action<SalesOrder> apply)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = status;
        apply(entity);
        return await _repository.UpdateAsync(entity);
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
            (CrmLineCalculator.CalculateLineTotal(l.Quantity, l.UnitPrice, l.DiscountPercent ?? 0), l.TaxRate ?? 0)));

        entity.SubTotal = totals.SubTotal;
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
        Status = dto.Status,
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
        if (!string.IsNullOrEmpty(dto.Status)) entity.Status = dto.Status;
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
