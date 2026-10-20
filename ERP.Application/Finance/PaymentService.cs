using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using IPaymentRepository = ERP.Infrastructure.Interfaces.Finance.IPaymentRepository;

namespace ERP.Application.Finance;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ERPDbContext? _db;

    public PaymentService(IPaymentRepository repository, IInvoiceRepository invoiceRepository, ERPDbContext? db = null)
    {
        _repository = repository;
        _invoiceRepository = invoiceRepository;
        _db = db;
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PaymentDto>();
        return (await _repository.GetAllAsync(tenantId)).Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreatePaymentDto dto)
    {
        if (dto.TenantId == Guid.Empty) return Guid.Empty;

        var entity = MapToEntity(dto);
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        if (string.IsNullOrWhiteSpace(entity.PaymentNumber))
            entity.PaymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{entity.Id:N}"[..20].ToUpperInvariant();

        var id = await _repository.CreateAsync(entity);
        if (id == Guid.Empty) return Guid.Empty;

        if (dto.RelatedInvoiceId.HasValue && dto.RelatedInvoiceId != Guid.Empty && (dto.Amount ?? 0) > 0)
        {
            await ApplyPaymentToInvoiceAsync(new PaymentApplicationDto
            {
                PaymentId = id,
                InvoiceId = dto.RelatedInvoiceId.Value,
                Amount = dto.Amount ?? 0
            }, dto.TenantId, Guid.TryParse(dto.CreatedBy, out var uid) ? uid : Guid.Empty);
        }
        else if (dto.RelatedBillId.HasValue && dto.RelatedBillId != Guid.Empty && (dto.Amount ?? 0) > 0)
        {
            await ApplyPaymentToBillAsync(new PaymentApplicationDto
            {
                PaymentId = id,
                BillId = dto.RelatedBillId.Value,
                Amount = dto.Amount ?? 0
            }, dto.TenantId, Guid.TryParse(dto.CreatedBy, out var uid) ? uid : Guid.Empty);
        }

        return id;
    }

    public async Task<bool> UpdateAsync(UpdatePaymentDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.PaymentId ?? dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdatePaymentDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.PaymentId ?? dto.Id, tenantId);
        if (entity == null) return false;
        dto.UpdatedBy = userId.ToString();
        MapToEntity(dto, entity);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
        => await _repository.DeleteAsync(id);

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        => tenantId != Guid.Empty && await _repository.DeleteAsync(id, tenantId);

    public async Task<IEnumerable<PaymentDto>> GetByInvoiceAsync(Guid invoiceId)
    {
        var entities = await _repository.GetByInvoiceAsync(invoiceId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetByInvoiceAsync(Guid invoiceId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PaymentDto>();
        return (await _repository.GetByInvoiceAsync(invoiceId, tenantId)).Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetByBillAsync(Guid billId)
    {
        var entities = await _repository.GetByBillAsync(billId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetByBillAsync(Guid billId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PaymentDto>();
        return (await _repository.GetByBillAsync(billId, tenantId)).Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PaymentDto>();
        return (await _repository.GetByDateRangeAsync(startDate, endDate, tenantId)).Select(MapToDto);
    }

    public async Task<bool> ProcessPaymentAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = "Completed";
        entity.ProcessedDate = DateTime.UtcNow;
        entity.ProcessedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<Guid> AddPaymentAsync(CreatePaymentDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        dto.TenantId = tenantId;
        dto.CreatedBy = userId.ToString();
        return await CreateAsync(dto);
    }

    public async Task<Guid> UpdatePaymentAsync(UpdatePaymentDto dto, Guid tenantId, Guid userId)
        => await UpdateAsync(dto, tenantId, userId) ? (dto.PaymentId ?? dto.Id) : Guid.Empty;

    public async Task<bool> ProcessPaymentAsync(Guid paymentId, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(paymentId, tenantId);
        if (entity == null) return false;

        entity.Status = "Completed";
        entity.ProcessedDate = DateTime.UtcNow;
        entity.ProcessedAt = DateTime.UtcNow;
        entity.ProcessedBy = userId;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> ReconcilePaymentAsync(Guid paymentId, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(paymentId, tenantId);
        if (entity == null) return false;

        entity.Status = "Reconciled";
        entity.UpdatedBy = userId.ToString();
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> ApplyPaymentToInvoiceAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || dto.Amount <= 0 || dto.InvoiceId == Guid.Empty) return false;

        var invoice = await _invoiceRepository.GetByIdAsync(dto.InvoiceId, tenantId);
        if (invoice == null || invoice.Balance <= 0) return false;

        var apply = Math.Min(dto.Amount, invoice.Balance);
        var ok = await _invoiceRepository.ApplyPaymentAsync(dto.InvoiceId, apply, tenantId);
        if (!ok) return false;

        var payment = await EnsurePaymentAsync(dto.PaymentId, tenantId, userId, apply, relatedInvoiceId: dto.InvoiceId);
        if (payment == null) return false;

        // Compat: keep RelatedInvoiceId for void/refund paths; multi-apply lives on PaymentApplications
        payment.RelatedInvoiceId = dto.InvoiceId;
        payment.Amount = Math.Max(payment.Amount, apply);
        payment.Status = "Completed";
        payment.ProcessedDate = DateTime.UtcNow;
        payment.ProcessedAt = DateTime.UtcNow;
        payment.ProcessedBy = userId;
        payment.UpdatedBy = userId.ToString();
        await _repository.UpdateAsync(payment);
        await PersistApplicationAsync(new PaymentApplication
        {
            TenantId = tenantId,
            PaymentId = payment.Id,
            InvoiceId = dto.InvoiceId,
            BillId = null,
            AppliedAmount = apply,
            AppliedDate = DateTime.UtcNow.Date,
            Status = "Applied",
            CreatedBy = userId.ToString()
        });
        return true;
    }

    public async Task<bool> ApplyPaymentToBillAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var billId = dto.BillId != Guid.Empty ? dto.BillId : dto.InvoiceId;
        if (dto.Amount <= 0 || billId == Guid.Empty) return false;

        var bill = await _invoiceRepository.GetBillByIdAsync(billId, tenantId);
        if (bill == null || bill.Balance <= 0) return false;

        var apply = Math.Min(dto.Amount, bill.Balance);
        var ok = await _invoiceRepository.ApplyPaymentToBillAsync(billId, apply, tenantId);
        if (!ok) return false;

        var payment = await EnsurePaymentAsync(dto.PaymentId, tenantId, userId, apply, relatedBillId: billId);
        if (payment == null) return false;

        payment.RelatedBillId = billId;
        payment.Status = "Completed";
        payment.ProcessedDate = DateTime.UtcNow;
        payment.ProcessedAt = DateTime.UtcNow;
        payment.ProcessedBy = userId;
        payment.UpdatedBy = userId.ToString();
        await _repository.UpdateAsync(payment);
        await PersistApplicationAsync(new PaymentApplication
        {
            TenantId = tenantId,
            PaymentId = payment.Id,
            InvoiceId = null,
            BillId = billId,
            AppliedAmount = apply,
            AppliedDate = DateTime.UtcNow.Date,
            Status = "Applied",
            CreatedBy = userId.ToString()
        });
        return true;
    }

    private async Task PersistApplicationAsync(PaymentApplication app)
    {
        if (_db == null) return;
        _db.PaymentApplications.Add(app);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> VoidAsync(Guid paymentId, VoidPaymentDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var payment = await _repository.GetByIdAsync(paymentId, tenantId);
        if (payment == null) return false;
        if (string.Equals(payment.Status, "Voided", StringComparison.OrdinalIgnoreCase))
            return false;

        if (payment.RelatedInvoiceId.HasValue && payment.RelatedInvoiceId != Guid.Empty && payment.Amount > 0)
        {
            var ok = await _invoiceRepository.UnapplyPaymentAsync(
                payment.RelatedInvoiceId.Value, payment.Amount, tenantId);
            if (!ok) return false;
        }
        else if (payment.RelatedBillId.HasValue && payment.RelatedBillId != Guid.Empty && payment.Amount > 0)
        {
            var ok = await _invoiceRepository.UnapplyPaymentFromBillAsync(
                payment.RelatedBillId.Value, payment.Amount, tenantId);
            if (!ok) return false;
        }

        payment.Status = "Voided";
        payment.IsActive = false;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        var reason = string.IsNullOrWhiteSpace(dto.Reason) ? dto.Notes : dto.Reason;
        if (!string.IsNullOrWhiteSpace(reason))
            payment.Notes = string.IsNullOrWhiteSpace(payment.Notes)
                ? $"Voided: {reason.Trim()}"
                : $"{payment.Notes}\nVoided: {reason.Trim()}";
        return await _repository.UpdateAsync(payment);
    }

    public async Task<bool> RefundAsync(Guid paymentId, RefundDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var payment = await _repository.GetByIdAsync(paymentId, tenantId);
        if (payment == null) return false;
        if (string.Equals(payment.Status, "Voided", StringComparison.OrdinalIgnoreCase)
            || string.Equals(payment.Status, "Refunded", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.Equals(payment.Status, "Completed", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(payment.Status, "Reconciled", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(payment.Status, "PartiallyRefunded", StringComparison.OrdinalIgnoreCase))
            return false;

        var amount = dto.RefundAmount > 0 ? dto.RefundAmount : payment.Amount;
        if (amount <= 0 || amount > payment.Amount + 0.01m) return false;
        amount = Math.Round(Math.Min(amount, payment.Amount), 2);

        if (payment.RelatedInvoiceId.HasValue && payment.RelatedInvoiceId != Guid.Empty)
        {
            var ok = await _invoiceRepository.UnapplyPaymentAsync(
                payment.RelatedInvoiceId.Value, amount, tenantId);
            if (!ok) return false;
        }
        else if (payment.RelatedBillId.HasValue && payment.RelatedBillId != Guid.Empty)
        {
            var ok = await _invoiceRepository.UnapplyPaymentFromBillAsync(
                payment.RelatedBillId.Value, amount, tenantId);
            if (!ok) return false;
        }

        var remaining = Math.Round(payment.Amount - amount, 2);
        payment.Amount = remaining;
        payment.Status = remaining <= 0.01m ? "Refunded" : "PartiallyRefunded";
        if (remaining <= 0.01m)
        {
            payment.Amount = 0;
            payment.IsActive = false;
        }
        payment.UpdatedAt = DateTime.UtcNow;
        payment.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        var reason = string.IsNullOrWhiteSpace(dto.Reason) ? dto.Notes : dto.Reason;
        if (!string.IsNullOrWhiteSpace(reason))
            payment.Notes = string.IsNullOrWhiteSpace(payment.Notes)
                ? $"Refunded {amount}: {reason.Trim()}"
                : $"{payment.Notes}\nRefunded {amount}: {reason.Trim()}";
        return await _repository.UpdateAsync(payment);
    }

    private async Task<Payment?> EnsurePaymentAsync(
        Guid paymentId,
        Guid tenantId,
        Guid userId,
        decimal amount,
        Guid? relatedInvoiceId = null,
        Guid? relatedBillId = null)
    {
        if (tenantId == Guid.Empty) return null;

        if (paymentId != Guid.Empty)
        {
            var existing = await _repository.GetByIdAsync(paymentId, tenantId);
            if (existing != null) return existing;
        }

        var id = Guid.NewGuid();
        var payment = new Payment
        {
            Id = id,
            TenantId = tenantId,
            PaymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{id:N}"[..20].ToUpperInvariant(),
            PaymentDate = DateTime.UtcNow,
            PaymentType = "Manual",
            PaymentMethod = "Manual",
            Amount = amount,
            Currency = "USD",
            CurrencyCode = "USD",
            RelatedInvoiceId = relatedInvoiceId,
            RelatedBillId = relatedBillId,
            Status = "Completed",
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        var created = await _repository.CreateAsync(payment);
        return created == Guid.Empty ? null : payment;
    }

    private static PaymentDto MapToDto(Payment entity)
    {
        return new PaymentDto
        {
            Id = entity.Id,
            PaymentId = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            PaymentNumber = entity.PaymentNumber,
            PaymentDate = entity.PaymentDate,
            PaymentType = entity.PaymentType,
            PaymentMethod = entity.PaymentMethod,
            Amount = entity.Amount,
            Currency = entity.Currency,
            CurrencyCode = entity.CurrencyCode,
            BankAccountId = entity.BankAccountId,
            RelatedInvoiceId = entity.RelatedInvoiceId,
            RelatedBillId = entity.RelatedBillId,
            Status = entity.Status,
            Reference = entity.Reference,
            ReferenceNumber = entity.ReferenceNumber,
            Notes = entity.Notes,
            ProcessedDate = entity.ProcessedDate,
            ProcessedAt = entity.ProcessedAt,
            ProcessedBy = entity.ProcessedBy,
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
            PaymentNumber = dto.PaymentNumber ?? string.Empty,
            PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
            PaymentType = dto.PaymentType ?? dto.PaymentMethod ?? string.Empty,
            PaymentMethod = dto.PaymentMethod ?? dto.PaymentType ?? string.Empty,
            Amount = dto.Amount ?? 0m,
            Currency = dto.Currency ?? "USD",
            CurrencyCode = dto.CurrencyCode ?? dto.Currency ?? "USD",
            BankAccountId = dto.BankAccountId,
            RelatedInvoiceId = dto.RelatedInvoiceId,
            RelatedBillId = dto.RelatedBillId,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
            Reference = dto.Reference,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void MapToEntity(UpdatePaymentDto dto, Payment entity)
    {
        entity.PaymentDate = dto.PaymentDate ?? entity.PaymentDate;
        if (!string.IsNullOrWhiteSpace(dto.PaymentNumber)) entity.PaymentNumber = dto.PaymentNumber;
        entity.PaymentType = dto.PaymentType ?? entity.PaymentType;
        entity.PaymentMethod = dto.PaymentMethod ?? entity.PaymentMethod;
        entity.Amount = dto.Amount ?? entity.Amount;
        if (!string.IsNullOrWhiteSpace(dto.Currency)) entity.Currency = dto.Currency;
        if (!string.IsNullOrWhiteSpace(dto.CurrencyCode)) entity.CurrencyCode = dto.CurrencyCode;
        if (dto.BankAccountId.HasValue) entity.BankAccountId = dto.BankAccountId;
        if (dto.RelatedInvoiceId.HasValue) entity.RelatedInvoiceId = dto.RelatedInvoiceId;
        if (dto.RelatedBillId.HasValue) entity.RelatedBillId = dto.RelatedBillId;
        // ERP-FIX-5: status is NOT free-set from dto; lifecycle endpoints govern it.
        entity.Reference = dto.Reference ?? entity.Reference;
        entity.ReferenceNumber = dto.ReferenceNumber ?? entity.ReferenceNumber;
        entity.Notes = dto.Notes ?? entity.Notes;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
