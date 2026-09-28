using ERP.Application.Interfaces.Sales;
using ERP.Domain.Sales;
using ERP.Infrastructure.Interfaces.Sales;
using ERP.Shared.DTOs.Sales;

namespace ERP.Application.Sales;

public class POSTransactionService : IPOSTransactionService
{
    private readonly IPOSTransactionRepository _repository;

    public POSTransactionService(IPOSTransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<POSTransactionDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<POSTransactionDto>> GetAllAsync(Guid tenantId)
    {
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreatePOSTransactionDto dto, Guid tenantId, Guid userId)
    {
        var entity = MapToEntity(dto, tenantId, userId);
        var created = await _repository.AddAsync(entity);
        return created.Id;
    }

    public async Task<bool> UpdateAsync(UpdatePOSTransactionDto dto, Guid tenantId, Guid userId)
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

    public async Task<IEnumerable<POSTransactionDto>> GetBySessionAsync(Guid sessionId, Guid tenantId)
    {
        var entities = await _repository.GetBySessionAsync(sessionId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<POSTransactionDto>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<POSTransactionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate, tenantId);
        return entities.Select(MapToDto);
    }

    private POSTransactionDto MapToDto(POSTransaction entity)
    {
        return new POSTransactionDto
        {
            Id = entity.Id,
            TransactionNumber = entity.TransactionNumber,
            POSSessionId = entity.POSSessionId,
            SessionId = entity.SessionId,
            CustomerId = entity.CustomerId,
            TransactionDate = entity.TransactionDate,
            PaymentMethod = entity.PaymentMethod,
            SubTotal = entity.SubTotal,
            TaxAmount = entity.TaxAmount,
            DiscountAmount = entity.DiscountAmount,
            TotalAmount = entity.TotalAmount,
            AmountPaid = entity.AmountPaid,
            ChangeAmount = entity.ChangeAmount,
            Status = entity.Status,
            Notes = entity.Notes,
            TenantId = entity.TenantId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private POSTransaction MapToEntity(CreatePOSTransactionDto dto, Guid tenantId, Guid userId)
    {
        return new POSTransaction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransactionNumber = dto.TransactionNumber,
            POSSessionId = dto.POSSessionId,
            SessionId = dto.SessionId,
            CustomerId = dto.CustomerId,
            TransactionDate = dto.TransactionDate,
            PaymentMethod = dto.PaymentMethod,
            SubTotal = dto.SubTotal,
            TaxAmount = dto.TaxAmount,
            DiscountAmount = dto.DiscountAmount,
            TotalAmount = dto.TotalAmount,
            AmountPaid = dto.AmountPaid,
            ChangeAmount = dto.TotalAmount - dto.AmountPaid,
            Status = dto.Status,
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private void MapToEntity(UpdatePOSTransactionDto dto, POSTransaction entity, Guid userId)
    {
        entity.TransactionNumber = dto.TransactionNumber;
        entity.SessionId = dto.SessionId;
        entity.CustomerId = dto.CustomerId;
        entity.TransactionDate = dto.TransactionDate;
        entity.PaymentMethod = dto.PaymentMethod;
        entity.SubTotal = dto.SubTotal;
        entity.TaxAmount = dto.TaxAmount;
        entity.DiscountAmount = dto.DiscountAmount;
        entity.TotalAmount = dto.TotalAmount;
        entity.AmountPaid = dto.AmountPaid;
        entity.ChangeAmount = dto.TotalAmount - dto.AmountPaid;
        entity.Status = dto.Status;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
    }
}

