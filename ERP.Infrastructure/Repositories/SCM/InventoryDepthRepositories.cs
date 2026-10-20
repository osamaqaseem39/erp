using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class BrandRepository : IBrandRepository
{
    private readonly ERPDbContext _db;
    public BrandRepository(ERPDbContext db) => _db = db;

    public Task<Brand?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.Brands.FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

    public async Task<IEnumerable<Brand>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var q = _db.Brands.Where(b => b.TenantId == tenantId);
        if (isActive.HasValue) q = q.Where(b => b.IsActive == isActive.Value);
        return await q.OrderBy(b => b.Name).ToListAsync();
    }

    public async Task<Brand> CreateAsync(Brand entity)
    {
        _db.Brands.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> UpdateAsync(Brand entity)
    {
        _db.Brands.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _db.Brands.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class WarehouseLocationRepository : IWarehouseLocationRepository
{
    private readonly ERPDbContext _db;
    public WarehouseLocationRepository(ERPDbContext db) => _db = db;

    public Task<WarehouseLocation?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.WarehouseLocations.FirstOrDefaultAsync(l => l.Id == id && l.TenantId == tenantId);

    public async Task<IEnumerable<WarehouseLocation>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId) =>
        await _db.WarehouseLocations
            .Where(l => l.TenantId == tenantId && l.WarehouseId == warehouseId && l.IsActive)
            .OrderBy(l => l.Code)
            .ToListAsync();

    public async Task<WarehouseLocation> CreateAsync(WarehouseLocation entity)
    {
        _db.WarehouseLocations.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> UpdateAsync(WarehouseLocation entity)
    {
        _db.WarehouseLocations.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _db.WarehouseLocations.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class StockReservationRepository : IStockReservationRepository
{
    private readonly ERPDbContext _db;
    public StockReservationRepository(ERPDbContext db) => _db = db;

    public Task<StockReservation?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.StockReservations.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

    public async Task<IEnumerable<StockReservation>> GetAllAsync(Guid tenantId) =>
        await _db.StockReservations.Include(r => r.Lines)
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<StockReservation>> GetBySalesOrderAsync(Guid salesOrderId, Guid tenantId) =>
        await _db.StockReservations.Include(r => r.Lines)
            .Where(r => r.TenantId == tenantId && r.SalesOrderId == salesOrderId)
            .ToListAsync();

    public async Task<Guid> CreateAsync(StockReservation entity)
    {
        _db.StockReservations.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(StockReservation entity)
    {
        _db.StockReservations.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.StockReservations.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(entity.Status, "Active", StringComparison.OrdinalIgnoreCase))
            return false;
        _db.StockReservationLines.RemoveRange(entity.Lines);
        _db.StockReservations.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class ItemKitRepository : IItemKitRepository
{
    private readonly ERPDbContext _db;
    public ItemKitRepository(ERPDbContext db) => _db = db;

    public Task<ItemKit?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.ItemKits.Include(k => k.Components)
            .FirstOrDefaultAsync(k => k.Id == id && k.TenantId == tenantId);

    public async Task<IEnumerable<ItemKit>> GetAllAsync(Guid tenantId) =>
        await _db.ItemKits.Include(k => k.Components)
            .Where(k => k.TenantId == tenantId)
            .OrderBy(k => k.Code)
            .ToListAsync();

    public async Task<Guid> CreateAsync(ItemKit entity)
    {
        _db.ItemKits.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(ItemKit entity)
    {
        _db.ItemKits.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.ItemKits.Include(k => k.Components)
            .FirstOrDefaultAsync(k => k.Id == id && k.TenantId == tenantId);
        if (entity == null) return false;
        _db.ItemKitComponents.RemoveRange(entity.Components);
        _db.ItemKits.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class CycleCountRepository : ICycleCountRepository
{
    private readonly ERPDbContext _db;
    public CycleCountRepository(ERPDbContext db) => _db = db;

    public Task<CycleCount?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.CycleCounts.Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

    public async Task<IEnumerable<CycleCount>> GetAllAsync(Guid tenantId) =>
        await _db.CycleCounts.Include(c => c.Lines)
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.CountDate)
            .ToListAsync();

    public async Task<Guid> CreateAsync(CycleCount entity)
    {
        _db.CycleCounts.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(CycleCount entity)
    {
        _db.CycleCounts.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.CycleCounts.Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase)) return false;
        _db.CycleCountLines.RemoveRange(entity.Lines);
        _db.CycleCounts.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}
