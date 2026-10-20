using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Manufacturing;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Manufacturing;

public interface IRoutingService
{
    Task<IReadOnlyList<RoutingDto>> ListAsync(Guid tenantId);
    Task<RoutingDto?> GetAsync(Guid id, Guid tenantId);
    Task<RoutingDto> CreateAsync(CreateRoutingDto dto, Guid tenantId, Guid userId);
}

public class RoutingService : IRoutingService
{
    private readonly ERPDbContext _db;

    public RoutingService(ERPDbContext db) => _db = db;

    public async Task<IReadOnlyList<RoutingDto>> ListAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<RoutingDto>();
        var rows = await _db.Routings.AsNoTracking()
            .Include(r => r.Steps)
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .OrderBy(r => r.RoutingNumber)
            .ToListAsync();
        return rows.Select(Map).ToList();
    }

    public async Task<RoutingDto?> GetAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _db.Routings.AsNoTracking()
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<RoutingDto> CreateAsync(CreateRoutingDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (string.IsNullOrWhiteSpace(dto.RoutingNumber) || string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Routing number and name are required.");

        var entity = new Routing
        {
            TenantId = tenantId,
            RoutingNumber = dto.RoutingNumber.Trim(),
            Name = dto.Name.Trim(),
            ItemId = dto.ItemId,
            Version = string.IsNullOrWhiteSpace(dto.Version) ? "1" : dto.Version.Trim(),
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        var n = 1;
        foreach (var step in dto.Steps.OrderBy(s => s.StepNumber))
        {
            entity.Steps.Add(new RoutingStep
            {
                TenantId = tenantId,
                StepNumber = step.StepNumber > 0 ? step.StepNumber : n++,
                WorkCenterId = step.WorkCenterId,
                OperationName = step.OperationName.Trim(),
                StandardTimeMinutes = step.StandardTimeMinutes,
                Notes = step.Notes,
                CreatedBy = userId.ToString()
            });
        }

        _db.Routings.Add(entity);
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    private static RoutingDto Map(Routing e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        RoutingNumber = e.RoutingNumber,
        Name = e.Name,
        ItemId = e.ItemId,
        Version = e.Version,
        Status = e.Status,
        Notes = e.Notes,
        Steps = e.Steps.OrderBy(s => s.StepNumber).Select(s => new RoutingStepDto
        {
            Id = s.Id,
            StepNumber = s.StepNumber,
            WorkCenterId = s.WorkCenterId,
            OperationName = s.OperationName,
            StandardTimeMinutes = s.StandardTimeMinutes,
            Notes = s.Notes
        }).ToList()
    };
}
