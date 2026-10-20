using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class StockAdjustmentRepository : IStockAdjustmentRepository
{
    private readonly ERPDbContext _db;

    public StockAdjustmentRepository(ERPDbContext db) => _db = db;

    public async Task<StockAdjustment?> GetByIdAsync(Guid id, Guid tenantId)
        => await _db.StockAdjustments
            .Include(a => a.Warehouse)
            .Include(a => a.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

    public async Task<IEnumerable<StockAdjustment>> GetAllAsync(Guid tenantId)
        => await _db.StockAdjustments.AsNoTracking()
            .Include(a => a.Warehouse)
            .Include(a => a.Lines)
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.AdjustmentDate)
            .ToListAsync();

    public async Task<IEnumerable<StockAdjustment>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId)
        => await _db.StockAdjustments.AsNoTracking()
            .Include(a => a.Lines)
            .Where(a => a.TenantId == tenantId && a.WarehouseId == warehouseId)
            .OrderByDescending(a => a.AdjustmentDate)
            .ToListAsync();

    public async Task<IEnumerable<StockAdjustment>> GetByItemAsync(Guid itemId, Guid tenantId)
        => await _db.StockAdjustments.AsNoTracking()
            .Include(a => a.Lines)
            .Where(a => a.TenantId == tenantId && a.Lines.Any(l => l.ItemId == itemId))
            .OrderByDescending(a => a.AdjustmentDate)
            .ToListAsync();

    public async Task<IEnumerable<StockAdjustment>> GetByDateRangeAsync(Guid tenantId, DateTime start, DateTime end)
        => await _db.StockAdjustments.AsNoTracking()
            .Include(a => a.Lines)
            .Where(a => a.TenantId == tenantId && a.AdjustmentDate >= start && a.AdjustmentDate <= end)
            .OrderByDescending(a => a.AdjustmentDate)
            .ToListAsync();

    public async Task<Guid> CreateAsync(StockAdjustment entity)
    {
        _db.StockAdjustments.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(StockAdjustment entity)
    {
        _db.StockAdjustments.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.StockAdjustments
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase)) return false;
        _db.StockAdjustmentLines.RemoveRange(entity.Lines);
        _db.StockAdjustments.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}
