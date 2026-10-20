using ERP.Application.Interfaces.Manufacturing;
using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Shared.DTOs.Manufacturing;

namespace ERP.Application.Manufacturing;

public class BillOfMaterialService : IBillOfMaterialService
{
    private readonly IBillOfMaterialRepository _repo;

    public BillOfMaterialService(IBillOfMaterialRepository repo) => _repo = repo;

    public async Task<BillOfMaterialDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<BillOfMaterialDto>> GetAllAsync(Guid tenantId, Guid? finishedGoodItemId = null)
        => (await _repo.GetAllAsync(tenantId, finishedGoodItemId)).Select(Map);

    public async Task<BillOfMaterialDto> CreateAsync(CreateBillOfMaterialDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.");
        ValidateHeader(dto.Name, dto.FinishedGoodItemId, dto.Lines);

        var id = Guid.NewGuid();
        var entity = new BillOfMaterial
        {
            Id = id,
            TenantId = tenantId,
            BomNumber = string.IsNullOrWhiteSpace(dto.BomNumber)
                ? $"BOM-{DateTime.UtcNow:yyyyMMdd}-{id.ToString("N")[..6]}"
                : dto.BomNumber.Trim(),
            Name = dto.Name.Trim(),
            FinishedGoodItemId = dto.FinishedGoodItemId,
            Version = string.IsNullOrWhiteSpace(dto.Version) ? "1" : dto.Version.Trim(),
            Status = "Draft",
            IsDefault = dto.IsDefault,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString(),
            IsActive = true,
            Lines = BuildLines(dto.Lines, id, tenantId, userId)
        };

        await _repo.CreateAsync(entity);
        return Map(entity);
    }

    public async Task<BillOfMaterialDto> UpdateAsync(UpdateBillOfMaterialDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.");
        var entity = await _repo.GetByIdAsync(dto.Id, tenantId)
            ?? throw new KeyNotFoundException($"BOM '{dto.Id}' not found.");
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Draft BOMs can be updated.");

        ValidateHeader(dto.Name, dto.FinishedGoodItemId, dto.Lines);

        entity.BomNumber = string.IsNullOrWhiteSpace(dto.BomNumber) ? entity.BomNumber : dto.BomNumber.Trim();
        entity.Name = dto.Name.Trim();
        entity.FinishedGoodItemId = dto.FinishedGoodItemId;
        entity.Version = string.IsNullOrWhiteSpace(dto.Version) ? entity.Version : dto.Version.Trim();
        if (dto.IsDefault.HasValue) entity.IsDefault = dto.IsDefault.Value;
        entity.EffectiveFrom = dto.EffectiveFrom ?? entity.EffectiveFrom;
        entity.EffectiveTo = dto.EffectiveTo ?? entity.EffectiveTo;
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();

        foreach (var old in entity.Lines.ToList())
            old.IsActive = false;

        foreach (var line in BuildLines(dto.Lines, entity.Id, tenantId, userId))
            entity.Lines.Add(line);

        await _repo.UpdateAsync(entity);
        return Map(await _repo.GetByIdAsync(entity.Id, tenantId) ?? entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (string.Equals(entity.Status, "Active", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Active BOMs cannot be deleted; obsolete them first.");
        return await _repo.SoftDeleteAsync(id, tenantId);
    }

    public async Task<BillOfMaterialDto> ActivateAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.");
        var entity = await _repo.GetByIdAsync(id, tenantId)
            ?? throw new KeyNotFoundException($"BOM '{id}' not found.");
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(entity.Status, "Obsolete", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Cannot activate BOM in status '{entity.Status}'.");
        if (!entity.Lines.Any(l => l.IsActive))
            throw new InvalidOperationException("BOM must have at least one component line to activate.");

        entity.Status = "Active";
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _repo.UpdateAsync(entity);
        return Map(entity);
    }

    public async Task<BillOfMaterialDto> ObsoleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.");
        var entity = await _repo.GetByIdAsync(id, tenantId)
            ?? throw new KeyNotFoundException($"BOM '{id}' not found.");
        if (!string.Equals(entity.Status, "Active", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Active BOMs can be obsoleted.");

        entity.Status = "Obsolete";
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _repo.UpdateAsync(entity);
        return Map(entity);
    }

    private static void ValidateHeader(string name, Guid fgItemId, IList<BillOfMaterialLineDto> lines)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("BOM name is required.");
        if (fgItemId == Guid.Empty)
            throw new ArgumentException("FinishedGoodItemId is required.");
        if (lines == null || lines.Count == 0)
            throw new ArgumentException("BOM requires at least one component line.");
        if (lines.Any(l => l.ComponentItemId == Guid.Empty || l.QuantityPer <= 0))
            throw new ArgumentException("Each line needs a component item and QuantityPer > 0.");
        if (lines.Any(l => l.ComponentItemId == fgItemId))
            throw new ArgumentException("Finished good cannot be a component of itself.");
    }

    private static List<BillOfMaterialLine> BuildLines(
        IList<BillOfMaterialLineDto> lines, Guid bomId, Guid tenantId, Guid userId)
    {
        var n = 0;
        return lines.Select(l => new BillOfMaterialLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BillOfMaterialId = bomId,
            LineNumber = l.LineNumber > 0 ? l.LineNumber : ++n,
            ComponentItemId = l.ComponentItemId,
            QuantityPer = l.QuantityPer,
            ScrapPercent = l.ScrapPercent < 0 ? 0 : l.ScrapPercent,
            AlternateGroup = string.IsNullOrWhiteSpace(l.AlternateGroup) ? null : l.AlternateGroup.Trim(),
            Notes = l.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString(),
            IsActive = true
        }).ToList();
    }

    private static BillOfMaterialDto Map(BillOfMaterial b) => new()
    {
        Id = b.Id,
        TenantId = b.TenantId,
        BomNumber = b.BomNumber,
        Name = b.Name,
        FinishedGoodItemId = b.FinishedGoodItemId,
        Version = b.Version,
        Status = b.Status,
        IsDefault = b.IsDefault,
        EffectiveFrom = b.EffectiveFrom,
        EffectiveTo = b.EffectiveTo,
        Notes = b.Notes,
        IsActive = b.IsActive,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt,
        CreatedBy = b.CreatedBy,
        UpdatedBy = b.UpdatedBy,
        Lines = b.Lines.Where(l => l.IsActive).OrderBy(l => l.LineNumber).Select(l => new BillOfMaterialLineDto
        {
            Id = l.Id,
            LineNumber = l.LineNumber,
            ComponentItemId = l.ComponentItemId,
            QuantityPer = l.QuantityPer,
            ScrapPercent = l.ScrapPercent,
            AlternateGroup = l.AlternateGroup,
            Notes = l.Notes
        }).ToList()
    };
}
