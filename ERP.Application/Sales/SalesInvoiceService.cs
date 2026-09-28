using ERP.Application.Interfaces.Sales;
using ERP.Domain.Sales;
using ERP.Infrastructure.Interfaces.Sales;
using ERP.Shared.DTOs.Sales;

namespace ERP.Application.Sales;

public class SalesInvoiceService : ISalesInvoiceService
{
    private readonly ISalesInvoiceRepository _repository;

    public SalesInvoiceService(ISalesInvoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<SalesInvoiceDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetAllAsync(Guid tenantId)
    {
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateSalesInvoiceDto dto, Guid tenantId, Guid userId)
    {
        var entity = MapToEntity(dto, tenantId, userId);
        var created = await _repository.AddAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdateSalesInvoiceDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;

        MapToEntity(dto, entity, userId);
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        var entities = await _repository.GetByStatusAsync(status, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<bool> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy)
    {
        return await _repository.PostInvoiceAsync(invoiceId, tenantId, postedBy);
    }

    public async Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid tenantId, Guid voidedBy)
    {
        return await _repository.VoidInvoiceAsync(invoiceId, tenantId, voidedBy);
    }

    private SalesInvoiceDto MapToDto(SalesInvoice entity)
    {
        return new SalesInvoiceDto
        {
            Id = entity.Id,
            InvoiceNumber = entity.InvoiceNumber,
            CustomerId = entity.CustomerId,
            InvoiceDate = entity.InvoiceDate,
            DueDate = entity.DueDate,
            Status = entity.Status,
            SubTotal = entity.SubTotal,
            TaxAmount = entity.TaxAmount,
            TotalAmount = entity.TotalAmount,
            PaidAmount = entity.PaidAmount,
            BalanceAmount = entity.BalanceAmount,
            Notes = entity.Notes,
            Terms = entity.Terms,
            SalesOrderId = entity.SalesOrderId,
            PostedAt = entity.PostedAt,
            PostedBy = entity.PostedBy,
            TenantId = entity.TenantId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private SalesInvoice MapToEntity(CreateSalesInvoiceDto dto, Guid tenantId, Guid userId)
    {
        return new SalesInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvoiceNumber = dto.InvoiceNumber,
            CustomerId = dto.CustomerId,
            InvoiceDate = dto.InvoiceDate,
            DueDate = dto.DueDate,
            Status = dto.Status,
            SubTotal = dto.SubTotal,
            TaxAmount = dto.TaxAmount,
            TotalAmount = dto.TotalAmount,
            Notes = dto.Notes,
            Terms = dto.Terms,
            SalesOrderId = dto.SalesOrderId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private void MapToEntity(UpdateSalesInvoiceDto dto, SalesInvoice entity, Guid userId)
    {
        entity.InvoiceNumber = dto.InvoiceNumber;
        entity.CustomerId = dto.CustomerId;
        entity.InvoiceDate = dto.InvoiceDate;
        entity.DueDate = dto.DueDate;
        entity.Status = dto.Status;
        entity.SubTotal = dto.SubTotal;
        entity.TaxAmount = dto.TaxAmount;
        entity.TotalAmount = dto.TotalAmount;
        entity.Notes = dto.Notes;
        entity.Terms = dto.Terms;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
    }
}

