using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public class BillOfMaterialRepository : IBillOfMaterialRepository
{
    private readonly ERPDbContext _db;

    public BillOfMaterialRepository(ERPDbContext db) => _db = db;

    public Task<BillOfMaterial?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.BillOfMaterials
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId && b.IsActive);

    public Task<BillOfMaterial?> GetActiveByFinishedGoodAsync(Guid finishedGoodItemId, Guid tenantId) =>
        _db.BillOfMaterials
            .Include(b => b.Lines)
            .Where(b => b.TenantId == tenantId && b.IsActive
                        && b.FinishedGoodItemId == finishedGoodItemId
                        && b.Status == "Active")
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<BillOfMaterial>> GetAllAsync(Guid tenantId, Guid? finishedGoodItemId = null)
    {
        var q = _db.BillOfMaterials
            .Include(b => b.Lines)
            .Where(b => b.TenantId == tenantId && b.IsActive);
        if (finishedGoodItemId.HasValue)
            q = q.Where(b => b.FinishedGoodItemId == finishedGoodItemId.Value);
        return await q.OrderBy(b => b.BomNumber).ToListAsync();
    }

    public async Task<Guid> CreateAsync(BillOfMaterial entity)
    {
        _db.BillOfMaterials.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(BillOfMaterial entity)
    {
        _db.BillOfMaterials.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> SoftDeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.BillOfMaterials.FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);
        if (entity == null || !entity.IsActive) return false;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _db.SaveChangesAsync() > 0;
    }
}
