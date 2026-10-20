using ERP.Application.Interfaces.Core;
using ERP.Domain.Core;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Core;

namespace ERP.Application.Core;

public class FiscalYearService : IFiscalYearService
{
    private readonly IFiscalYearRepository _repo;

    public FiscalYearService(IFiscalYearRepository repo) => _repo = repo;

    public async Task<Guid> CreateAsync(CreateFiscalYearDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (string.IsNullOrWhiteSpace(dto.Name)) return Guid.Empty;

        var start = dto.StartDate.Date;
        var end = dto.EndDate.Date;
        if (end < start) return Guid.Empty;
        if (await _repo.HasOverlapAsync(tenantId, start, end)) return Guid.Empty;

        var status = NormalizeStatus(dto.Status) ?? "Active";
        var id = Guid.NewGuid();
        var entity = new FiscalYear
        {
            Id = id,
            FiscalYearId = id,
            TenantId = tenantId,
            Name = dto.Name.Trim(),
            StartDate = start,
            EndDate = end,
            Status = status,
            IsActive = string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase),
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        return await _repo.CreateAsync(entity);
    }

    public async Task<Guid> UpdateAsync(UpdateFiscalYearDto dto, Guid tenantId, Guid userId)
    {
        var id = dto.FiscalYearId ?? dto.Id;
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null) return Guid.Empty;

        var start = dto.StartDate?.Date ?? entity.StartDate;
        var end = dto.EndDate?.Date ?? entity.EndDate;
        if (end < start) return Guid.Empty;
        if (await _repo.HasOverlapAsync(tenantId, start, end, entity.FiscalYearId)) return Guid.Empty;

        if (!string.IsNullOrWhiteSpace(dto.Name)) entity.Name = dto.Name.Trim();
        entity.StartDate = start;
        entity.EndDate = end;
        if (dto.Status != null)
        {
            var status = NormalizeStatus(dto.Status);
            if (status == null) return Guid.Empty;
            entity.Status = status;
            entity.IsActive = string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase);
        }
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
        return entity.FiscalYearId;
    }

    public async Task<bool> CloseAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (string.Equals(entity.Status, "Closed", StringComparison.OrdinalIgnoreCase)) return true;
        entity.Status = "Closed";
        entity.IsActive = false;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        => await _repo.DeleteAsync(id, tenantId);

    public async Task<FiscalYearDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<FiscalYearDto>> GetAllAsync(Guid tenantId, string? status = null)
    {
        var list = await _repo.GetAllAsync(tenantId, status);
        return list.Select(Map);
    }

    public async Task<FiscalYearDto?> GetActiveCoveringAsync(Guid tenantId, DateTime date)
    {
        var entity = await _repo.FindActiveCoveringAsync(tenantId, date);
        return entity == null ? null : Map(entity);
    }

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return null;
        var s = status.Trim();
        if (string.Equals(s, "Active", StringComparison.OrdinalIgnoreCase)) return "Active";
        if (string.Equals(s, "Closed", StringComparison.OrdinalIgnoreCase)) return "Closed";
        return null;
    }

    private static FiscalYearDto Map(FiscalYear e) => new()
    {
        Id = e.Id,
        FiscalYearId = e.FiscalYearId,
        TenantId = e.TenantId,
        Name = e.Name,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        Status = e.Status,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
