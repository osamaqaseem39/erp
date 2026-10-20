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
        if (tenantId == Guid.Empty) return null;
        return await _context.StockBalances
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<StockBalance>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<StockBalance>();
        return await _context.StockBalances
            .Where(x => x.TenantId == tenantId)
            .ToListAsync();
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
        if (tenantId == Guid.Empty) return null;
        return await _context.StockBalances
            .FirstOrDefaultAsync(x => x.ItemId == itemId && x.WarehouseId == warehouseId && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<StockBalance>> GetByItemAsync(Guid itemId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<StockBalance>();
        return await _context.StockBalances
            .Where(x => x.ItemId == itemId && x.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockBalance>> GetByWarehouseAsync(Guid warehouseId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<StockBalance>();
        return await _context.StockBalances
            .Where(x => x.WarehouseId == warehouseId && x.TenantId == tenantId)
            .ToListAsync();
    }
}

public class StockTransferRepository : IStockTransferRepository
{
    private readonly ERPDbContext _context;

    public StockTransferRepository(ERPDbContext context) => _context = context;

    public async Task<StockTransfer?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        // Avoid Include() on required warehouse/company FKs — missing related rows
        // become INNER JOINs and hide the transfer. Load details separately.
        var entity = await _context.StockTransfers
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (entity == null) return null;

        await _context.Entry(entity).Collection(x => x.StockTransferDetails).LoadAsync();
        await _context.Entry(entity).Reference(x => x.FromWarehouse).LoadAsync();
        await _context.Entry(entity).Reference(x => x.ToWarehouse).LoadAsync();
        foreach (var line in entity.StockTransferDetails)
            await _context.Entry(line).Reference(d => d.Item).LoadAsync();
        return entity;
    }

    public async Task<IEnumerable<StockTransfer>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<StockTransfer>();
        return await _context.StockTransfers
            .Include(x => x.StockTransferDetails)
            .Where(x => x.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<StockTransfer> CreateAsync(StockTransfer entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        foreach (var line in entity.StockTransferDetails)
        {
            if (line.Id == Guid.Empty) line.Id = Guid.NewGuid();
            line.TransferId = entity.Id;
            line.TenantId = entity.TenantId;
            line.CreatedAt = DateTime.UtcNow;
        }
        _context.StockTransfers.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<StockTransfer> UpdateAsync(StockTransfer entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>Replace transfer lines (Draft edits). Caller must not keep stale detail instances tracked.</summary>
    public async Task ReplaceDetailsAsync(Guid transferId, Guid tenantId, IReadOnlyList<StockTransferDetail> lines)
    {
        if (tenantId == Guid.Empty) return;

        var existing = await _context.StockTransferDetails
            .Where(d => d.TransferId == transferId && d.TenantId == tenantId)
            .ToListAsync();
        _context.StockTransferDetails.RemoveRange(existing);
        await _context.SaveChangesAsync();

        foreach (var line in lines)
        {
            if (line.Id == Guid.Empty) line.Id = Guid.NewGuid();
            line.TransferId = transferId;
            line.TenantId = tenantId;
            if (line.CreatedAt == default) line.CreatedAt = DateTime.UtcNow;
            line.IsActive = true;
            _context.StockTransferDetails.Add(line);
        }
        await _context.SaveChangesAsync();
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
        if (tenantId == Guid.Empty) return Array.Empty<StockTransfer>();
        return await _context.StockTransfers
            .Where(x => x.TenantId == tenantId
                        && (x.FromWarehouseId == warehouseId || x.ToWarehouseId == warehouseId))
            .ToListAsync();
    }

    public async Task<IEnumerable<StockTransfer>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<StockTransfer>();
        return await _context.StockTransfers
            .Where(x => x.TenantId == tenantId && x.Status == status)
            .ToListAsync();
    }
}
