using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Manufacturing;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Manufacturing;

public interface IQualityCheckService
{
    Task<IReadOnlyList<QualityCheckDto>> ListAsync(Guid tenantId, Guid? productionOrderId = null);
    Task<QualityCheckDto> CreateAsync(CreateQualityCheckDto dto, Guid tenantId, Guid userId);
    Task<QualityCheckDto> PassAsync(Guid id, PassFailQualityCheckDto? dto, Guid tenantId, Guid userId);
    Task<QualityCheckDto> FailAsync(Guid id, PassFailQualityCheckDto? dto, Guid tenantId, Guid userId);
}

public class QualityCheckService : IQualityCheckService
{
    private readonly ERPDbContext _db;

    public QualityCheckService(ERPDbContext db) => _db = db;

    public async Task<IReadOnlyList<QualityCheckDto>> ListAsync(Guid tenantId, Guid? productionOrderId = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<QualityCheckDto>();
        var q = _db.QualityChecks.AsNoTracking()
            .Include(c => c.Lines)
            .Where(c => c.TenantId == tenantId && c.IsActive);
        if (productionOrderId.HasValue && productionOrderId != Guid.Empty)
            q = q.Where(c => c.ProductionOrderId == productionOrderId);
        return (await q.OrderByDescending(c => c.CreatedAt).ToListAsync()).Select(Map).ToList();
    }

    public async Task<QualityCheckDto> CreateAsync(CreateQualityCheckDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (dto.ProductionOrderId == Guid.Empty) throw new ValidationException("Production order is required.");

        var po = await _db.ProductionOrders
            .FirstOrDefaultAsync(p => p.Id == dto.ProductionOrderId && p.TenantId == tenantId)
            ?? throw new NotFoundException("Production order not found.");

        if (po.Status is not ("InProgress" or "QualityCheck"))
            throw new ValidationException("Quality checks can only be created for InProgress or QualityCheck orders.");

        var entity = new QualityCheck
        {
            TenantId = tenantId,
            ProductionOrderId = dto.ProductionOrderId,
            CheckNumber = $"QC-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Status = "Pending",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        foreach (var line in dto.Lines)
        {
            entity.Lines.Add(new QualityCheckLine
            {
                TenantId = tenantId,
                ItemId = line.ItemId ?? po.ItemId,
                QuantityInspected = line.QuantityInspected,
                QuantityPassed = line.QuantityPassed,
                QuantityFailed = line.QuantityFailed,
                Notes = line.Notes,
                CreatedBy = userId.ToString()
            });
        }

        if (string.Equals(po.Status, "InProgress", StringComparison.OrdinalIgnoreCase))
        {
            po.Status = "QualityCheck";
            po.UpdatedAt = DateTime.UtcNow;
            po.UpdatedBy = userId.ToString();
        }

        _db.QualityChecks.Add(entity);
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    public Task<QualityCheckDto> PassAsync(Guid id, PassFailQualityCheckDto? dto, Guid tenantId, Guid userId) =>
        SetStatusAsync(id, "Passed", dto, tenantId, userId);

    public Task<QualityCheckDto> FailAsync(Guid id, PassFailQualityCheckDto? dto, Guid tenantId, Guid userId) =>
        SetStatusAsync(id, "Failed", dto, tenantId, userId);

    private async Task<QualityCheckDto> SetStatusAsync(
        Guid id, string status, PassFailQualityCheckDto? dto, Guid tenantId, Guid userId)
    {
        var entity = await _db.QualityChecks.Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId)
            ?? throw new NotFoundException("Quality check not found.");
        if (!string.Equals(entity.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException($"Quality check is already {entity.Status}.");

        entity.Status = status;
        entity.CheckedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto?.Notes)) entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    private static QualityCheckDto Map(QualityCheck e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ProductionOrderId = e.ProductionOrderId,
        CheckNumber = e.CheckNumber,
        Status = e.Status,
        CheckedAt = e.CheckedAt,
        Notes = e.Notes,
        Lines = e.Lines.Where(l => l.IsActive).Select(l => new QualityCheckLineDto
        {
            Id = l.Id,
            ItemId = l.ItemId,
            QuantityInspected = l.QuantityInspected,
            QuantityPassed = l.QuantityPassed,
            QuantityFailed = l.QuantityFailed,
            Notes = l.Notes
        }).ToList()
    };
}
