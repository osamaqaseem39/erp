using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public interface IAccountingPeriodService
{
    Task<IReadOnlyList<AccountingPeriodDto>> ListAsync(Guid tenantId, Guid? fiscalYearId = null);
    Task<AccountingPeriodDto?> GetAsync(Guid id, Guid tenantId);
    Task<AccountingPeriodDto> CreateAsync(CreateAccountingPeriodDto dto, Guid tenantId, Guid userId);
    Task<AccountingPeriodDto> CloseAsync(Guid id, Guid tenantId, Guid userId);
    Task<AccountingPeriodDto> OpenAsync(Guid id, Guid tenantId, Guid userId);
}

public class AccountingPeriodService : IAccountingPeriodService
{
    private readonly ERPDbContext _db;

    public AccountingPeriodService(ERPDbContext db) => _db = db;

    public async Task<IReadOnlyList<AccountingPeriodDto>> ListAsync(Guid tenantId, Guid? fiscalYearId = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<AccountingPeriodDto>();
        var q = _db.AccountingPeriods.AsNoTracking().Where(p => p.TenantId == tenantId && p.IsActive);
        if (fiscalYearId.HasValue && fiscalYearId != Guid.Empty)
            q = q.Where(p => p.FiscalYearId == fiscalYearId);
        return (await q.OrderBy(p => p.StartDate).ToListAsync()).Select(Map).ToList();
    }

    public async Task<AccountingPeriodDto?> GetAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _db.AccountingPeriods.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<AccountingPeriodDto> CreateAsync(CreateAccountingPeriodDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (dto.FiscalYearId == Guid.Empty) throw new ValidationException("Fiscal year is required.");
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ValidationException("Name is required.");
        if (dto.EndDate < dto.StartDate) throw new ValidationException("End date must be on or after start date.");

        var entity = new AccountingPeriod
        {
            TenantId = tenantId,
            FiscalYearId = dto.FiscalYearId,
            Name = dto.Name.Trim(),
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            Status = "Open",
            CreatedBy = userId.ToString()
        };
        _db.AccountingPeriods.Add(entity);
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<AccountingPeriodDto> CloseAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetTrackedAsync(id, tenantId);
        entity.Status = "Closed";
        entity.IsLocked = true;
        entity.ClosedAt = DateTime.UtcNow;
        entity.ClosedBy = userId == Guid.Empty ? null : userId;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<AccountingPeriodDto> OpenAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetTrackedAsync(id, tenantId);
        entity.Status = "Open";
        entity.IsLocked = false;
        entity.ClosedAt = null;
        entity.ClosedBy = null;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    private async Task<AccountingPeriod> GetTrackedAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        return await _db.AccountingPeriods.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId)
            ?? throw new NotFoundException("Accounting period not found.");
    }

    private static AccountingPeriodDto Map(AccountingPeriod e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        FiscalYearId = e.FiscalYearId,
        Name = e.Name,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        Status = e.Status,
        IsLocked = e.IsLocked,
        ClosedAt = e.ClosedAt,
        ClosedBy = e.ClosedBy
    };
}
