using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Manufacturing;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Manufacturing;

public interface IScrapReasonService
{
    Task<IReadOnlyList<ScrapReasonDto>> ListAsync(Guid tenantId);
    Task<ScrapReasonDto> CreateAsync(CreateScrapReasonDto dto, Guid tenantId, Guid userId);
}

public class ScrapReasonService : IScrapReasonService
{
    private readonly ERPDbContext _db;

    public ScrapReasonService(ERPDbContext db) => _db = db;

    public async Task<IReadOnlyList<ScrapReasonDto>> ListAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ScrapReasonDto>();
        return await _db.ScrapReasons.AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .OrderBy(r => r.Code)
            .Select(r => new ScrapReasonDto
            {
                Id = r.Id,
                TenantId = r.TenantId,
                Code = r.Code,
                Name = r.Name,
                IsActive = r.IsActive
            })
            .ToListAsync();
    }

    public async Task<ScrapReasonDto> CreateAsync(CreateScrapReasonDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Code and name are required.");

        var exists = await _db.ScrapReasons.AnyAsync(r =>
            r.TenantId == tenantId && r.Code == dto.Code.Trim());
        if (exists) throw new ValidationException("Scrap reason code already exists.");

        var entity = new ScrapReason
        {
            TenantId = tenantId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            CreatedBy = userId.ToString()
        };
        _db.ScrapReasons.Add(entity);
        await _db.SaveChangesAsync();
        return new ScrapReasonDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Code = entity.Code,
            Name = entity.Name,
            IsActive = entity.IsActive
        };
    }
}
