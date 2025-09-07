using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Domain.CRM;

namespace ERP.Application.CRM;

public class SalesOrderService : ISalesOrderService
{
    private readonly ISalesOrderRepository _repository;

    public SalesOrderService(ISalesOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<SalesOrderDto?> GetByIdAsync(Guid id)
    {
        // Note: This method needs tenantId, but it's not available in the interface
        // For now, using a default tenantId - this should be fixed properly
        var entity = await _repository.GetByIdAsync(id, Guid.Empty);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<SalesOrderDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync(Guid.Empty);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateSalesOrderDto dto)
    {
        var entity = MapToEntity(dto);
        var createdEntity = await _repository.AddAsync(entity);
        return createdEntity.Id;
    }

    public async Task<bool> UpdateAsync(UpdateSalesOrderDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, Guid.Empty);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id, Guid.Empty);
        return true;
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByCustomerAsync(Guid customerId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId, Guid.Empty);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByStatusAsync(string status)
    {
        var entities = await _repository.GetByStatusAsync(status, Guid.Empty);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate, Guid.Empty);
        return entities.Select(MapToDto);
    }

    public async Task<bool> ApproveAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id, Guid.Empty);
        if (entity == null) return false;

        entity.Status = "Approved";
        entity.ApprovedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> ShipAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id, Guid.Empty);
        if (entity == null) return false;

        entity.Status = "Shipped";
        entity.ShippedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> InvoiceAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id, Guid.Empty);
        if (entity == null) return false;

        entity.Status = "Invoiced";
        entity.InvoicedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    private static SalesOrderDto MapToDto(SalesOrder entity)
    {
        return new SalesOrderDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            OrderNumber = entity.OrderNumber,
            CustomerId = entity.CustomerId,
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
            IsActive = entity.IsActive
        };
    }

    private static SalesOrder MapToEntity(CreateSalesOrderDto dto)
    {
        return new SalesOrder
        {
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId,
            OrderNumber = dto.OrderNumber,
            CustomerId = dto.CustomerId,
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
    }

    private static void MapToEntity(UpdateSalesOrderDto dto, SalesOrder entity)
    {
        if (dto.CustomerId.HasValue)
            entity.CustomerId = dto.CustomerId.Value;
        if (dto.OrderDate.HasValue)
            entity.OrderDate = dto.OrderDate.Value;
        if (dto.RequiredDate.HasValue)
            entity.RequiredDate = dto.RequiredDate.Value;
        if (!string.IsNullOrEmpty(dto.Status))
            entity.Status = dto.Status;
        if (dto.SubTotal.HasValue)
            entity.SubTotal = dto.SubTotal.Value;
        if (dto.TaxAmount.HasValue)
            entity.TaxAmount = dto.TaxAmount.Value;
        if (dto.TotalAmount.HasValue)
            entity.TotalAmount = dto.TotalAmount.Value;
        if (!string.IsNullOrEmpty(dto.Currency))
            entity.Currency = dto.Currency;
        if (!string.IsNullOrEmpty(dto.Notes))
            entity.Notes = dto.Notes;
        if (!string.IsNullOrEmpty(dto.Terms))
            entity.Terms = dto.Terms;
    }
}