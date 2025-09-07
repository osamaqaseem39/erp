using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Infrastructure.Repositories;
using ERP.Domain.Finance;

namespace ERP.Application.Finance;

public class InvoiceService : IInvoiceService
{
    private readonly ERP.Application.Interfaces.Finance.IInvoiceRepository _repository;

    public InvoiceService(ERP.Application.Interfaces.Finance.IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> AddInvoiceAsync(InvoiceCreateDto dto, Guid tenantId, Guid userId)
    {
        var entity = MapToEntity(dto, tenantId, userId);
        var createdEntity = await _repository.AddAsync(entity);
        return createdEntity.InvoiceId;
    }

    public async Task<Guid> UpdateInvoiceAsync(InvoiceUpdateDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return Guid.Empty;

        MapToEntity(dto, entity, userId);
        await _repository.UpdateAsync(entity);
        return entity.InvoiceId;
    }

    public async Task<bool> DeleteInvoiceAsync(Guid invoiceId, Guid tenantId)
    {
        return await _repository.DeleteAsync(invoiceId, tenantId);
    }

    public async Task<Guid> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy)
    {
        var success = await _repository.PostInvoiceAsync(invoiceId, tenantId, postedBy);
        return success ? invoiceId : Guid.Empty;
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(invoiceId, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<(IEnumerable<InvoiceDto> Invoices, int TotalCount)> SearchInvoicesAsync(InvoiceQuery query)
    {
        var entities = await _repository.GetAllAsync(query.TenantId, query.CustomerId, query.Status, query.StartDate, query.EndDate, query.PageNumber, query.PageSize);
        var totalCount = await _repository.GetCountAsync(query.TenantId, query.CustomerId, query.Status, query.StartDate, query.EndDate);

        return (entities.Select(MapToDto), totalCount);
    }

    public async Task<Guid> AddInvoiceLineAsync(InvoiceLineCreateDto dto, Guid invoiceId, Guid tenantId, Guid userId)
    {
        // Implementation for invoice lines
        return Guid.NewGuid();
    }

    public async Task<Guid> UpdateInvoiceLineAsync(InvoiceLineUpdateDto dto, Guid tenantId, Guid userId)
    {
        // Implementation for invoice lines
        return Guid.NewGuid();
    }

    public async Task<bool> DeleteInvoiceLineAsync(Guid invoiceLineId, Guid tenantId)
    {
        // Implementation for invoice lines
        return true;
    }

    public async Task<IEnumerable<InvoiceLineDto>> GetInvoiceLinesByInvoiceIdAsync(Guid invoiceId, Guid tenantId)
    {
        // Implementation for invoice lines
        return new List<InvoiceLineDto>();
    }

    public async Task<Guid> AddBillAsync(BillCreateDto dto, Guid tenantId, Guid userId)
    {
        // Implementation for bills
        return Guid.NewGuid();
    }

    public async Task<Guid> UpdateBillAsync(BillUpdateDto dto, Guid tenantId, Guid userId)
    {
        // Implementation for bills
        return Guid.NewGuid();
    }

    public async Task<bool> DeleteBillAsync(Guid billId, Guid tenantId)
    {
        // Implementation for bills
        return true;
    }

    public async Task<Guid> PostBillAsync(Guid billId, Guid tenantId, Guid postedBy)
    {
        // Implementation for bills
        return Guid.NewGuid();
    }

    public async Task<BillDto?> GetBillByIdAsync(Guid billId, Guid tenantId)
    {
        // Implementation for bills
        return null;
    }

    public async Task<(IEnumerable<BillDto> Bills, int TotalCount)> SearchBillsAsync(BillQuery query)
    {
        // Implementation for bills
        return (new List<BillDto>(), 0);
    }

    public async Task<Guid> AddBillLineAsync(BillLineCreateDto dto, Guid billId, Guid tenantId, Guid userId)
    {
        // Implementation for bill lines
        return Guid.NewGuid();
    }

    public async Task<Guid> UpdateBillLineAsync(BillLineUpdateDto dto, Guid tenantId, Guid userId)
    {
        // Implementation for bill lines
        return Guid.NewGuid();
    }

    public async Task<bool> DeleteBillLineAsync(Guid billLineId, Guid tenantId)
    {
        // Implementation for bill lines
        return true;
    }

    public async Task<IEnumerable<BillLineDto>> GetBillLinesByBillIdAsync(Guid billId, Guid tenantId)
    {
        // Implementation for bill lines
        return new List<BillLineDto>();
    }

    public async Task<bool> ApplyPaymentToInvoiceAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId)
    {
        // Implementation for payment application
        return true;
    }

    public async Task<bool> ApplyPaymentToBillAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId)
    {
        // Implementation for payment application
        return true;
    }

    public async Task<object> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null)
    {
        // Implementation for aged receivables
        return new object();
    }

    public async Task<object> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null)
    {
        // Implementation for aged payables
        return new object();
    }

    public async Task<object> GetCustomerAgingAsync(Guid customerId, Guid tenantId, DateTime asOfDate)
    {
        // Implementation for customer aging
        return new object();
    }

    public async Task<object> GetSupplierAgingAsync(Guid supplierId, Guid tenantId, DateTime asOfDate)
    {
        // Implementation for supplier aging
        return new object();
    }

    private static InvoiceDto MapToDto(Invoice entity)
    {
        return new InvoiceDto
        {
            InvoiceId = entity.InvoiceId,
            TenantId = entity.TenantId,
            CustomerId = entity.CustomerId,
            InvoiceNumber = entity.InvoiceNumber,
            InvoiceDate = entity.InvoiceDate,
            DueDate = entity.DueDate,
            CurrencyCode = entity.CurrencyCode,
            SubTotal = entity.SubTotal,
            TaxTotal = entity.TaxTotal,
            TotalAmount = entity.TotalAmount,
            Balance = entity.Balance,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            SalesOrderId = entity.SalesOrderId,
            PostedAt = entity.PostedAt,
            PostedBy = entity.PostedBy,
            IsLocked = entity.IsLocked
        };
    }

    private static Invoice MapToEntity(InvoiceCreateDto dto, Guid tenantId, Guid userId)
    {
        return new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = dto.CustomerId,
            InvoiceNumber = dto.InvoiceNumber ?? string.Empty,
            InvoiceDate = dto.InvoiceDate ?? DateTime.UtcNow,
            DueDate = dto.DueDate ?? DateTime.UtcNow.AddDays(30),
            CurrencyCode = dto.CurrencyCode ?? "USD",
            SubTotal = dto.SubTotal ?? 0m,
            TaxTotal = dto.TaxAmount ?? 0m,
            TotalAmount = dto.TotalAmount ?? 0m,
            Status = dto.Status ?? "Draft",
            SalesOrderId = dto.SalesOrderId,
            IsLocked = dto.IsLocked ?? false,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void MapToEntity(InvoiceUpdateDto dto, Invoice entity, Guid userId)
    {
        if (dto.CustomerId.HasValue) entity.CustomerId = dto.CustomerId.Value;
        entity.InvoiceNumber = dto.InvoiceNumber ?? entity.InvoiceNumber;
        entity.InvoiceDate = dto.InvoiceDate ?? entity.InvoiceDate;
        entity.DueDate = dto.DueDate ?? entity.DueDate;
        entity.CurrencyCode = dto.CurrencyCode ?? entity.CurrencyCode;
        entity.Status = dto.Status ?? entity.Status;
        if (dto.SalesOrderId.HasValue) entity.SalesOrderId = dto.SalesOrderId.Value;
        entity.IsLocked = dto.IsLocked ?? entity.IsLocked;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
    }
}