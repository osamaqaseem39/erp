using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.HRM;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.HRM;

public interface ITeamService
{
    Task<IReadOnlyList<TeamDto>> ListAsync(Guid tenantId, bool? isActive = null);
    Task<TeamDto?> GetAsync(Guid id, Guid tenantId);
    Task<TeamDto> CreateAsync(CreateTeamDto dto, Guid tenantId, Guid userId);
    Task<TeamDto> UpdateAsync(Guid id, UpdateTeamDto dto, Guid tenantId, Guid userId);
}

public class TeamService : ITeamService
{
    private readonly ERPDbContext _db;

    public TeamService(ERPDbContext db) => _db = db;

    public async Task<IReadOnlyList<TeamDto>> ListAsync(Guid tenantId, bool? isActive = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<TeamDto>();
        var q = _db.Teams.AsNoTracking().Where(t => t.TenantId == tenantId);
        if (isActive.HasValue) q = q.Where(t => t.IsActive == isActive.Value);
        return (await q.OrderBy(t => t.Name).ToListAsync()).Select(Map).ToList();
    }

    public async Task<TeamDto?> GetAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _db.Teams.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<TeamDto> CreateAsync(CreateTeamDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ValidationException("Name is required.");
        if (dto.ManagerEmployeeId.HasValue && dto.ManagerEmployeeId != Guid.Empty)
        {
            var mgr = await _db.Employees.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == dto.ManagerEmployeeId && e.TenantId == tenantId);
            if (mgr == null) throw new NotFoundException("Manager employee not found.");
        }

        var entity = new Team
        {
            TenantId = tenantId,
            Name = dto.Name.Trim(),
            ManagerEmployeeId = dto.ManagerEmployeeId,
            CreatedBy = userId.ToString()
        };
        _db.Teams.Add(entity);
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<TeamDto> UpdateAsync(Guid id, UpdateTeamDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _db.Teams.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId)
            ?? throw new NotFoundException("Team not found.");
        if (!string.IsNullOrWhiteSpace(dto.Name)) entity.Name = dto.Name.Trim();
        if (dto.ManagerEmployeeId.HasValue)
        {
            if (dto.ManagerEmployeeId != Guid.Empty)
            {
                var mgr = await _db.Employees.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == dto.ManagerEmployeeId && e.TenantId == tenantId);
                if (mgr == null) throw new NotFoundException("Manager employee not found.");
            }
            entity.ManagerEmployeeId = dto.ManagerEmployeeId == Guid.Empty ? null : dto.ManagerEmployeeId;
        }
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    private static TeamDto Map(Team e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        Name = e.Name,
        ManagerEmployeeId = e.ManagerEmployeeId,
        IsActive = e.IsActive
    };
}
