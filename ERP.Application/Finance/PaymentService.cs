using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces.Finance;
using ERP.Domain.Finance;
using IPaymentRepository = ERP.Infrastructure.Interfaces.Finance.IPaymentRepository;

namespace ERP.Application.Finance;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;

    public PaymentService(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreatePaymentDto dto)
    {
        var entity = MapToEntity(dto);
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdatePaymentDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<PaymentDto>> GetByInvoiceAsync(Guid invoiceId)
    {
        var entities = await _repository.GetByInvoiceAsync(invoiceId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetByBillAsync(Guid billId)
    {
        var entities = await _repository.GetByBillAsync(billId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate);
        return entities.Select(MapToDto);
    }

    public async Task<bool> ProcessPaymentAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = "Processed";
        entity.ProcessedDate = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    // Additional methods for GraphQL mutations
    public async Task<Guid> AddPaymentAsync(CreatePaymentDto dto, Guid tenantId, Guid userId)
    {
        dto.TenantId = tenantId;
        dto.CreatedBy = userId.ToString();
        return await CreateAsync(dto);
    }

    public async Task<Guid> UpdatePaymentAsync(UpdatePaymentDto dto, Guid tenantId, Guid userId)
    {
        dto.UpdatedBy = userId.ToString();
        return await UpdateAsync(dto) ? dto.Id : Guid.Empty;
    }

    public async Task<bool> ProcessPaymentAsync(Guid paymentId, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(paymentId);
        if (entity == null || entity.TenantId != tenantId) return false;

        entity.Status = "Processed";
        entity.ProcessedDate = DateTime.UtcNow;
        entity.ProcessedBy = userId;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> ReconcilePaymentAsync(Guid paymentId, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(paymentId);
        if (entity == null || entity.TenantId != tenantId) return false;

        entity.Status = "Reconciled";
        entity.UpdatedBy = userId.ToString();
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> ApplyPaymentToInvoiceAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId)
    {
        // Implementation for applying payment to invoice
        // This would typically involve updating the payment and invoice records
        return await Task.FromResult(true);
    }

    public async Task<bool> ApplyPaymentToBillAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId)
    {
        // Implementation for applying payment to bill
        // This would typically involve updating the payment and bill records
        return await Task.FromResult(true);
    }

    private static PaymentDto MapToDto(Payment entity)
    {
        return new PaymentDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            PaymentNumber = entity.PaymentNumber,
            PaymentDate = entity.PaymentDate,
            PaymentType = entity.PaymentType,
            Amount = entity.Amount,
            Currency = entity.Currency,
            Status = entity.Status,
            Reference = entity.Reference,
            Notes = entity.Notes,
            ProcessedDate = entity.ProcessedDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static Payment MapToEntity(CreatePaymentDto dto)
    {
        return new Payment
        {
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId,
            PaymentNumber = dto.PaymentNumber,
            PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
            PaymentType = dto.PaymentType ?? string.Empty,
            Amount = dto.Amount ?? 0m,
            Currency = dto.Currency,
            Status = dto.Status,
            Reference = dto.Reference,
            Notes = dto.Notes,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdatePaymentDto dto, Payment entity)
    {
        entity.PaymentDate = dto.PaymentDate ?? entity.PaymentDate;
        entity.PaymentType = dto.PaymentType ?? entity.PaymentType;
        entity.Amount = dto.Amount ?? entity.Amount;
        entity.Currency = dto.Currency;
        entity.Status = dto.Status;
        entity.Reference = dto.Reference;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = dto.UpdatedBy;
    }
}