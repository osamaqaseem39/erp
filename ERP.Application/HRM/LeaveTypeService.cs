using ERP.Application.Interfaces.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.HRM;

public class LeaveTypeService : ILeaveTypeService
{
    private readonly ERPDbContext _db;

    public LeaveTypeService(ERPDbContext db) => _db = db;

    public async Task<LeaveTypeDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _db.LeaveTypes.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == tenantId);
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<LeaveTypeDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<LeaveTypeDto>();
        return (await _db.LeaveTypes.AsNoTracking()
            .Where(l => l.TenantId == tenantId)
            .OrderBy(l => l.Name)
            .ToListAsync()).Select(Map);
    }

    public async Task<LeaveTypeDto> CreateAsync(CreateLeaveTypeDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new InvalidOperationException("Tenant is required.");
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(dto.Code)) throw new ArgumentException("Code is required.");

        var entity = new LeaveType
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim().ToUpperInvariant(),
            IsPaid = dto.IsPaid,
            MaxAllowedPerYear = dto.MaxAllowedPerYear,
            RequiresApproval = dto.RequiresApproval,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString()
        };
        _db.LeaveTypes.Add(entity);
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<LeaveTypeDto> UpdateAsync(UpdateLeaveTypeDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new InvalidOperationException("Tenant is required.");
        var entity = await _db.LeaveTypes.FirstOrDefaultAsync(l => l.Id == dto.Id && l.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"Leave type '{dto.Id}' not found.");

        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(dto.Code)) throw new ArgumentException("Code is required.");

        entity.Name = dto.Name.Trim();
        entity.Code = dto.Code.Trim().ToUpperInvariant();
        entity.IsPaid = dto.IsPaid;
        entity.MaxAllowedPerYear = dto.MaxAllowedPerYear;
        entity.RequiresApproval = dto.RequiresApproval;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _db.LeaveTypes.FirstOrDefaultAsync(l => l.Id == id && l.TenantId == tenantId);
        if (entity == null) return false;
        _db.LeaveTypes.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    private static LeaveTypeDto Map(LeaveType e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Code = e.Code,
        IsPaid = e.IsPaid,
        MaxAllowedPerYear = e.MaxAllowedPerYear,
        RequiresApproval = e.RequiresApproval,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy
    };
}
