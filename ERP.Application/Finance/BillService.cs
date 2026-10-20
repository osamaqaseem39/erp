using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Finance;

public class BillService : IBillService
{
    private readonly IBillRepository _repository;
    private readonly IPaymentService? _payments;

    public BillService(IBillRepository repository, IPaymentService? payments = null)
    {
        _repository = repository;
        _payments = payments;
    }

    public async Task<BillDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId) return null;
        return MapToDto(entity);
    }

    public async Task<IEnumerable<BillDto>> GetAllAsync(
        Guid tenantId,
        Guid? vendorId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<BillDto>();

        IEnumerable<Bill> entities;
        if (vendorId.HasValue)
            entities = await _repository.GetBySupplierAsync(vendorId.Value);
        else if (!string.IsNullOrWhiteSpace(status))
            entities = await _repository.GetByStatusAsync(status);
        else if (fromDate.HasValue && toDate.HasValue)
            entities = await _repository.GetByDateRangeAsync(fromDate.Value, toDate.Value);
        else
            entities = await _repository.GetAllAsync();

        return entities
            .Where(b => b.TenantId == tenantId)
            .Where(b => !vendorId.HasValue || b.VendorId == vendorId || b.SupplierId == vendorId)
            .Where(b => string.IsNullOrWhiteSpace(status) || b.Status == status)
            .Select(MapToDto);
    }

    public async Task<BillDto> CreateAsync(CreateBillDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant is required.");
        var id = Guid.NewGuid();
        var entity = new Bill
        {
            Id = id,
            BillId = id,
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            VendorId = dto.VendorId,
            SupplierId = dto.VendorId,
            BillNumber = string.IsNullOrWhiteSpace(dto.BillNumber)
                ? $"BILL-{DateTime.UtcNow:yyyyMMdd}-{id:N}"[..20].ToUpperInvariant()
                : dto.BillNumber,
            BillDate = dto.BillDate == default ? DateTime.UtcNow : dto.BillDate,
            DueDate = dto.DueDate == default ? DateTime.UtcNow.AddDays(30) : dto.DueDate,
            CurrencyCode = dto.CurrencyCode ?? "USD",
            SubTotal = dto.SubTotal,
            TaxAmount = dto.TaxAmount,
            TaxTotal = dto.TaxAmount,
            TotalAmount = dto.TotalAmount,
            Balance = dto.TotalAmount,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Issued" : dto.Status,
            PurchaseOrderId = dto.PurchaseOrderId,
            Notes = BuildNotes(dto),
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsActive = dto.IsActive
        };

        await _repository.CreateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<BillDto> UpdateAsync(UpdateBillDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new InvalidOperationException("Tenant is required.");
        var entity = await _repository.GetByIdAsync(dto.BillId != Guid.Empty ? dto.BillId : dto.Id);
        if (entity == null || entity.TenantId != tenantId)
            throw new InvalidOperationException("Bill not found");

        if (!string.IsNullOrWhiteSpace(dto.BillNumber)) entity.BillNumber = dto.BillNumber;
        if (dto.VendorId.HasValue)
        {
            entity.VendorId = dto.VendorId.Value;
            entity.SupplierId = dto.VendorId.Value;
        }
        if (dto.BillDate.HasValue) entity.BillDate = dto.BillDate.Value;
        if (dto.DueDate.HasValue) entity.DueDate = dto.DueDate.Value;
        if (dto.SubTotal.HasValue) entity.SubTotal = dto.SubTotal.Value;
        if (dto.TaxAmount.HasValue)
        {
            entity.TaxAmount = dto.TaxAmount.Value;
            entity.TaxTotal = dto.TaxAmount.Value;
        }
        if (dto.TotalAmount.HasValue)
        {
            entity.TotalAmount = dto.TotalAmount.Value;
            entity.Balance = dto.TotalAmount.Value;
        }
        // ERP-FIX-5: status is NOT free-set from dto; lifecycle endpoints (Post/Pay/Void) govern it.
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (!string.IsNullOrWhiteSpace(dto.CurrencyCode)) entity.CurrencyCode = dto.CurrencyCode;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId) return false;
        if (string.Equals(entity.Status, "Paid", StringComparison.OrdinalIgnoreCase)) return false;
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<BillDto>> GetByVendorAsync(Guid vendorId, Guid tenantId) =>
        await GetAllAsync(tenantId, vendorId: vendorId);

    public async Task<IEnumerable<BillDto>> GetByStatusAsync(string status, Guid tenantId) =>
        await GetAllAsync(tenantId, status: status);

    public async Task<IEnumerable<BillDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId) =>
        await GetAllAsync(tenantId, fromDate: startDate, toDate: endDate);

    public async Task<BillDto> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new InvalidOperationException("Tenant is required.");
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId)
            throw new InvalidOperationException("Bill not found");

        entity.Status = "Issued";
        entity.ApprovedDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<BillDto> MarkAsPaidAsync(Guid id, PaymentDto paymentDto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new InvalidOperationException("Tenant is required.");
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId)
            throw new InvalidOperationException("Bill not found");
        if (entity.Balance <= 0)
            return MapToDto(entity);

        if (_payments == null)
            throw new InvalidOperationException("Payment service is required to mark a bill as paid.");

        var amount = paymentDto.Amount is > 0 ? paymentDto.Amount.Value : entity.Balance;
        var apply = Math.Min(amount, entity.Balance);
        var ok = await _payments.ApplyPaymentToBillAsync(new PaymentApplicationDto
        {
            PaymentId = paymentDto.PaymentId ?? paymentDto.Id,
            BillId = id,
            Amount = apply,
            ApplicationDate = paymentDto.PaymentDate ?? DateTime.UtcNow,
            Notes = paymentDto.Notes ?? paymentDto.Reference
        }, tenantId, userId);
        if (!ok)
            throw new InvalidOperationException("Could not apply payment to bill.");

        var updated = await GetByIdAsync(id, tenantId);
        return updated ?? MapToDto(entity);
    }

    public async Task<bool> ApplyCreditToBillAsync(Guid billId, decimal amount, Guid tenantId)
    {
        if (tenantId == Guid.Empty || amount <= 0 || billId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(billId);
        if (entity == null || entity.TenantId != tenantId)
            return false;
        if (string.Equals(entity.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "Voided", StringComparison.OrdinalIgnoreCase))
            return false;
        if (amount > entity.Balance) return false;

        entity.Balance = Math.Round(entity.Balance - amount, 2);
        entity.Status = entity.Balance <= 0 ? "Paid" : "PartiallyPaid";
        if (entity.Balance <= 0) entity.PaidDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> UnapplyCreditFromBillAsync(Guid billId, decimal amount, Guid tenantId)
    {
        if (tenantId == Guid.Empty || amount <= 0 || billId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(billId);
        if (entity == null || entity.TenantId != tenantId)
            return false;
        if (string.Equals(entity.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "Voided", StringComparison.OrdinalIgnoreCase))
            return false;

        entity.Balance = Math.Round(entity.Balance + amount, 2);
        if (entity.Balance > entity.TotalAmount) entity.Balance = entity.TotalAmount;
        entity.Status = entity.Balance >= entity.TotalAmount ? "Issued" : "PartiallyPaid";
        if (entity.Balance > 0) entity.PaidDate = null;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<BillDto> VoidBillAsync(Guid id, VoidBillDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new InvalidOperationException("Tenant is required.");
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId)
            throw new InvalidOperationException("Bill not found");
        if (string.Equals(entity.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "Voided", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Bill already voided");

        var applied = Math.Round(entity.TotalAmount - entity.Balance, 2);
        if (applied > 0.01m)
            throw new InvalidOperationException("Void applied payments before voiding the bill");

        entity.Status = "Cancelled";
        entity.Balance = 0;
        entity.IsLocked = true;
        entity.IsActive = false;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Reason))
            entity.Notes = string.IsNullOrWhiteSpace(entity.Notes)
                ? $"Voided: {dto.Reason}"
                : $"{entity.Notes}\nVoided: {dto.Reason}";
        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    private static string BuildNotes(CreateBillDto dto)
    {
        var notes = dto.Notes ?? string.Empty;
        if (dto.PurchaseInvoiceId.HasValue && dto.PurchaseInvoiceId != Guid.Empty)
        {
            var tag = $"PurchaseInvoice:{dto.PurchaseInvoiceId}";
            if (!notes.Contains(tag, StringComparison.OrdinalIgnoreCase))
                notes = string.IsNullOrWhiteSpace(notes) ? tag : $"{notes}\n{tag}";
        }
        if (!string.IsNullOrWhiteSpace(dto.Reference))
            notes = string.IsNullOrWhiteSpace(notes) ? $"Ref:{dto.Reference}" : $"{notes}\nRef:{dto.Reference}";
        return notes;
    }

    private static BillDto MapToDto(Bill entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        CompanyId = entity.CompanyId,
        BillNumber = entity.BillNumber,
        VendorId = entity.VendorId != Guid.Empty ? entity.VendorId : entity.SupplierId,
        BillDate = entity.BillDate,
        DueDate = entity.DueDate,
        SubTotal = entity.SubTotal,
        TaxAmount = entity.TaxAmount != 0 ? entity.TaxAmount : entity.TaxTotal,
        TotalAmount = entity.TotalAmount,
        Balance = entity.Balance,
        Status = entity.Status,
        Notes = entity.Notes,
        ApprovedDate = entity.ApprovedDate,
        PaidDate = entity.PaidDate,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy,
        IsActive = entity.IsActive
    };
}
