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

    public Task<ItemKit?> GetByKitItemIdAsync(Guid kitItemId, Guid tenantId) =>
        _db.ItemKits.Include(k => k.Components)
            .FirstOrDefaultAsync(k => k.KitItemId == kitItemId && k.TenantId == tenantId && k.IsActive);

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

public class PutawayTaskRepository : IPutawayTaskRepository
{
    private readonly ERPDbContext _db;
    public PutawayTaskRepository(ERPDbContext db) => _db = db;

    public Task<PutawayTask?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.PutawayTasks.Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

    public async Task<IEnumerable<PutawayTask>> GetAllAsync(Guid tenantId) =>
        await _db.PutawayTasks.Include(t => t.Lines)
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public Task<PutawayTask?> GetByReferenceAsync(string referenceDocType, Guid referenceDocId, Guid tenantId) =>
        _db.PutawayTasks.Include(t => t.Lines)
            .FirstOrDefaultAsync(t =>
                t.TenantId == tenantId
                && t.ReferenceDocType == referenceDocType
                && t.ReferenceDocId == referenceDocId
                && t.Status != "Cancelled");

    public async Task<Guid> CreateAsync(PutawayTask entity)
    {
        _db.PutawayTasks.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(PutawayTask entity)
    {
        _db.PutawayTasks.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.PutawayTasks.Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Open", StringComparison.OrdinalIgnoreCase)) return false;
        _db.PutawayTaskLines.RemoveRange(entity.Lines);
        _db.PutawayTasks.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class PickListRepository : IPickListRepository
{
    private readonly ERPDbContext _db;
    public PickListRepository(ERPDbContext db) => _db = db;

    public Task<PickList?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.PickLists.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

    public async Task<IEnumerable<PickList>> GetAllAsync(Guid tenantId) =>
        await _db.PickLists.Include(p => p.Lines)
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public Task<PickList?> GetCompletedByReferenceAsync(string referenceDocType, Guid referenceDocId, Guid tenantId) =>
        _db.PickLists.Include(p => p.Lines)
            .FirstOrDefaultAsync(p =>
                p.TenantId == tenantId
                && p.ReferenceDocType == referenceDocType
                && p.ReferenceDocId == referenceDocId
                && p.Status == "Completed");

    public async Task<IEnumerable<PickList>> GetByReferenceAsync(string referenceDocType, Guid referenceDocId, Guid tenantId) =>
        await _db.PickLists.Include(p => p.Lines)
            .Where(p =>
                p.TenantId == tenantId
                && p.ReferenceDocType == referenceDocType
                && p.ReferenceDocId == referenceDocId)
            .ToListAsync();

    public async Task<Guid> CreateAsync(PickList entity)
    {
        _db.PickLists.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(PickList entity)
    {
        _db.PickLists.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.PickLists.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Open", StringComparison.OrdinalIgnoreCase)) return false;
        _db.PickListLines.RemoveRange(entity.Lines);
        _db.PickLists.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class ReplenishmentTaskRepository : IReplenishmentTaskRepository
{
    private readonly ERPDbContext _db;
    public ReplenishmentTaskRepository(ERPDbContext db) => _db = db;

    public Task<ReplenishmentTask?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.ReplenishmentTasks.Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

    public async Task<IEnumerable<ReplenishmentTask>> GetAllAsync(Guid tenantId) =>
        await _db.ReplenishmentTasks.Include(t => t.Lines)
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<Guid> CreateAsync(ReplenishmentTask entity)
    {
        _db.ReplenishmentTasks.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(ReplenishmentTask entity)
    {
        _db.ReplenishmentTasks.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.ReplenishmentTasks.Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Open", StringComparison.OrdinalIgnoreCase)) return false;
        _db.ReplenishmentTaskLines.RemoveRange(entity.Lines);
        _db.ReplenishmentTasks.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class PackOrderRepository : IPackOrderRepository
{
    private readonly ERPDbContext _db;
    public PackOrderRepository(ERPDbContext db) => _db = db;

    public Task<PackOrder?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.PackOrders.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

    public async Task<IEnumerable<PackOrder>> GetAllAsync(Guid tenantId) =>
        await _db.PackOrders.Include(p => p.Lines)
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public Task<PackOrder?> GetPackedByDispatchAsync(Guid dispatchAdviceId, Guid tenantId) =>
        _db.PackOrders.Include(p => p.Lines)
            .FirstOrDefaultAsync(p =>
                p.TenantId == tenantId
                && p.DispatchAdviceId == dispatchAdviceId
                && p.Status == "Packed");

    public async Task<Guid> CreateAsync(PackOrder entity)
    {
        _db.PackOrders.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(PackOrder entity)
    {
        _db.PackOrders.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.PackOrders.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Open", StringComparison.OrdinalIgnoreCase)) return false;
        _db.PackOrderLines.RemoveRange(entity.Lines);
        _db.PackOrders.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class ShipmentRepository : IShipmentRepository
{
    private readonly ERPDbContext _db;
    public ShipmentRepository(ERPDbContext db) => _db = db;

    public Task<Shipment?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.Shipments.Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

    public async Task<IEnumerable<Shipment>> GetAllAsync(Guid tenantId) =>
        await _db.Shipments.Include(s => s.Lines)
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

    public Task<Shipment?> GetShippedByDispatchAsync(Guid dispatchAdviceId, Guid tenantId) =>
        _db.Shipments.Include(s => s.Lines)
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId
                && s.DispatchAdviceId == dispatchAdviceId
                && s.Status == "Shipped");

    public async Task<Guid> CreateAsync(Shipment entity)
    {
        _db.Shipments.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Shipment entity)
    {
        _db.Shipments.Update(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _db.Shipments.Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase)) return false;
        _db.ShipmentLines.RemoveRange(entity.Lines);
        _db.Shipments.Remove(entity);
        return await _db.SaveChangesAsync() > 0;
    }
}
