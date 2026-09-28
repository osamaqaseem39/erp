using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class StockBalanceRepository : IStockBalanceRepository
{
    private readonly ERPDbContext _context;

    public StockBalanceRepository(ERPDbContext context) => _context = context;

    public async Task<StockBalance?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var q = _context.StockBalances.AsQueryable();
        if (tenantId != Guid.Empty) q = q.Where(x => x.TenantId == tenantId);
        return await q.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<StockBalance>> GetAllAsync(Guid tenantId)
    {
        var q = _context.StockBalances.AsQueryable();
        if (tenantId != Guid.Empty) q = q.Where(x => x.TenantId == tenantId);
        return await q.ToListAsync();
    }

    public async Task<StockBalance> CreateAsync(StockBalance entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        _context.StockBalances.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<StockBalance> UpdateAsync(StockBalance entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.StockBalances.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _context.StockBalances.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<StockBalance?> GetByItemAndWarehouseAsync(Guid itemId, Guid warehouseId, Guid tenantId)
    {
        var q = _context.StockBalances.Where(x => x.ItemId == itemId && x.WarehouseId == warehouseId);
        if (tenantId != Guid.Empty) q = q.Where(x => x.TenantId == tenantId);
        return await q.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<StockBalance>> GetByItemAsync(Guid itemId, Guid tenantId)
    {
        var q = _context.StockBalances.Where(x => x.ItemId == itemId);
        if (tenantId != Guid.Empty) q = q.Where(x => x.TenantId == tenantId);
        return await q.ToListAsync();
    }

    public async Task<IEnumerable<StockBalance>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId)
    {
        var q = _context.StockBalances.Where(x => x.WarehouseId == warehouseId);
        if (tenantId != Guid.Empty) q = q.Where(x => x.TenantId == tenantId);
        return await q.ToListAsync();
    }
}

public class StockTransferRepository : IStockTransferRepository
{
    private readonly ERPDbContext _context;

    public StockTransferRepository(ERPDbContext context) => _context = context;

    public async Task<StockTransfer?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var q = _context.StockTransfers.AsQueryable();
        if (tenantId != Guid.Empty) q = q.Where(x => x.TenantId == tenantId);
        return await q.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<StockTransfer>> GetAllAsync(Guid tenantId)
    {
        var q = _context.StockTransfers.AsQueryable();
        if (tenantId != Guid.Empty) q = q.Where(x => x.TenantId == tenantId);
        return await q.ToListAsync();
    }

    public async Task<StockTransfer> CreateAsync(StockTransfer entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        _context.StockTransfers.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<StockTransfer> UpdateAsync(StockTransfer entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.StockTransfers.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        _context.StockTransfers.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<StockTransfer>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId)
    {
        var q = _context.StockTransfers
            .Where(x => x.FromWarehouseId == warehouseId || x.ToWarehouseId == warehouseId);
        if (tenantId != Guid.Empty) q = q.Where(x => x.TenantId == tenantId);
        return await q.ToListAsync();
    }

    public async Task<IEnumerable<StockTransfer>> GetByStatusAsync(string status, Guid tenantId)
    {
        var q = _context.StockTransfers.Where(x => x.Status == status);
        if (tenantId != Guid.Empty) q = q.Where(x => x.TenantId == tenantId);
        return await q.ToListAsync();
    }
}
