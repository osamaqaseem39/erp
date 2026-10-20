using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Finance;

public class CostCenterService : ICostCenterService
{
    private readonly ICostCenterRepository _repo;

    public CostCenterService(ICostCenterRepository repo) => _repo = repo;

    public async Task<Guid> CreateAsync(CreateCostCenterDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty
            || string.IsNullOrWhiteSpace(dto.CostCenterCode)
            || string.IsNullOrWhiteSpace(dto.CostCenterName))
            return Guid.Empty;

        var code = dto.CostCenterCode.Trim();
        if (await _repo.GetByCodeAsync(code, tenantId) != null) return Guid.Empty;

        var id = Guid.NewGuid();
        var entity = new CostCenter
        {
            Id = id,
            CostCenterId = id,
            TenantId = tenantId,
            CostCenterCode = code,
            CostCenterName = dto.CostCenterName.Trim(),
            Description = dto.Description,
            IsActive = dto.IsActive ?? true,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        return await _repo.CreateAsync(entity);
    }

    public async Task<Guid> UpdateAsync(UpdateCostCenterDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var id = dto.CostCenterId ?? dto.Id;
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null) return Guid.Empty;

        if (!string.IsNullOrWhiteSpace(dto.CostCenterCode)
            && !string.Equals(dto.CostCenterCode.Trim(), entity.CostCenterCode, StringComparison.OrdinalIgnoreCase))
        {
            var clash = await _repo.GetByCodeAsync(dto.CostCenterCode.Trim(), tenantId);
            if (clash != null && clash.CostCenterId != entity.CostCenterId) return Guid.Empty;
            entity.CostCenterCode = dto.CostCenterCode.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.CostCenterName)) entity.CostCenterName = dto.CostCenterName.Trim();
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
        return entity.CostCenterId;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        => tenantId != Guid.Empty && await _repo.DeleteAsync(id, tenantId);

    public async Task<bool> SoftDeactivateAsync(Guid id, Guid tenantId)
        => tenantId != Guid.Empty && await _repo.SoftDeactivateAsync(id, tenantId);

    public async Task<CostCenterDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repo.GetByIdAsync(id, tenantId);
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<CostCenterDto>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<CostCenterDto>();
        var list = await _repo.GetAllAsync(tenantId, isActive);
        return list.Select(Map);
    }

    private static CostCenterDto Map(CostCenter e) => new()
    {
        Id = e.Id,
        CostCenterId = e.CostCenterId,
        TenantId = e.TenantId,
        CostCenterCode = e.CostCenterCode,
        CostCenterName = e.CostCenterName,
        Description = e.Description,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
