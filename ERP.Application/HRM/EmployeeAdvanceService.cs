using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.HRM;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using EmployeeAdvanceEntity = ERP.Domain.HRM.EmployeeAdvance;

namespace ERP.Application.HRM;

public interface IEmployeeAdvanceService
{
    Task<IReadOnlyList<EmployeeAdvanceDto>> ListAsync(Guid tenantId, Guid? employeeId = null);
    Task<EmployeeAdvanceDto?> GetAsync(Guid id, Guid tenantId);
    Task<EmployeeAdvanceDto> DisburseAsync(CreateEmployeeAdvanceDto dto, Guid tenantId, Guid userId);
    Task<EmployeeAdvanceDto> RecoverAsync(Guid id, RecoverEmployeeAdvanceDto dto, Guid tenantId, Guid userId);
}

public class EmployeeAdvanceService : IEmployeeAdvanceService
{
    private readonly ERPDbContext _db;

    public EmployeeAdvanceService(ERPDbContext db) => _db = db;

    public async Task<IReadOnlyList<EmployeeAdvanceDto>> ListAsync(Guid tenantId, Guid? employeeId = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<EmployeeAdvanceDto>();
        var q = _db.EmployeeAdvances.AsNoTracking().Where(a => a.TenantId == tenantId && a.IsActive);
        if (employeeId.HasValue && employeeId != Guid.Empty)
            q = q.Where(a => a.EmployeeId == employeeId);
        return (await q.OrderByDescending(a => a.AdvanceDate).ToListAsync()).Select(Map).ToList();
    }

    public async Task<EmployeeAdvanceDto?> GetAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _db.EmployeeAdvances.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<EmployeeAdvanceDto> DisburseAsync(CreateEmployeeAdvanceDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (dto.EmployeeId == Guid.Empty) throw new ValidationException("Employee is required.");
        if (dto.Amount <= 0) throw new ValidationException("Amount must be positive.");

        var emp = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && e.TenantId == tenantId);
        if (emp == null) throw new NotFoundException("Employee not found.");

        var refNo = string.IsNullOrWhiteSpace(dto.ReferenceNo)
            ? $"ADV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant()
            : dto.ReferenceNo.Trim();

        var entity = new EmployeeAdvanceEntity
        {
            TenantId = tenantId,
            EmployeeId = dto.EmployeeId,
            ReferenceNo = refNo,
            AdvanceDate = dto.AdvanceDate == default ? DateTime.UtcNow.Date : dto.AdvanceDate.Date,
            Amount = dto.Amount,
            RecoveredAmount = 0,
            Status = "Disbursed",
            Notes = dto.Notes,
            DisbursedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };
        _db.EmployeeAdvances.Add(entity);
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<EmployeeAdvanceDto> RecoverAsync(Guid id, RecoverEmployeeAdvanceDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (dto.Amount <= 0) throw new ValidationException("Recovery amount must be positive.");

        var entity = await _db.EmployeeAdvances
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId)
            ?? throw new NotFoundException("Advance not found.");

        var balance = entity.Amount - entity.RecoveredAmount;
        if (dto.Amount > balance + 0.01m)
            throw new ValidationException("Recovery exceeds outstanding balance.");

        entity.RecoveredAmount = Math.Round(entity.RecoveredAmount + dto.Amount, 2);
        if (entity.RecoveredAmount >= entity.Amount)
            entity.Status = "Recovered";
        else
            entity.Status = "PartiallyRecovered";
        if (dto.Notes != null)
            entity.Notes = string.IsNullOrWhiteSpace(entity.Notes) ? dto.Notes : $"{entity.Notes}\n{dto.Notes}";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    private static EmployeeAdvanceDto Map(EmployeeAdvanceEntity e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        EmployeeId = e.EmployeeId,
        ReferenceNo = e.ReferenceNo,
        AdvanceDate = e.AdvanceDate,
        Amount = e.Amount,
        RecoveredAmount = e.RecoveredAmount,
        Balance = Math.Round(e.Amount - e.RecoveredAmount, 2),
        Status = e.Status,
        Notes = e.Notes
    };
}
