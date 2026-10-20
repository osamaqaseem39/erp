using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class StockLedgerRepository : IStockLedgerRepository
{
    private readonly ERPDbContext _context;

    public StockLedgerRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<StockLedger?> GetByIdAsync(Guid id)
        => await _context.StockLedgers
            .Include(l => l.Item)
            .Include(l => l.Warehouse)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IEnumerable<StockLedger>> GetAllAsync()
        => await _context.StockLedgers
            .Include(l => l.Item)
            .Include(l => l.Warehouse)
            .OrderByDescending(l => l.TransactionDate)
            .ToListAsync();

    public async Task<IEnumerable<StockLedger>> GetByItemAsync(
        Guid itemId, Guid tenantId, Guid? warehouseId = null, DateTime? toDate = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<StockLedger>();
        var q = _context.StockLedgers.AsNoTracking()
            .Include(l => l.Item)
            .Include(l => l.Warehouse)
            .Where(l => l.ItemId == itemId && l.TenantId == tenantId);
        if (warehouseId.HasValue && warehouseId != Guid.Empty)
            q = q.Where(l => l.WarehouseId == warehouseId);
        if (toDate.HasValue)
        {
            var endInclusive = toDate.Value.Date.AddDays(1).AddTicks(-1);
            q = q.Where(l => l.TransactionDate <= endInclusive);
        }
        return await q
            .OrderBy(l => l.TransactionDate)
            .ThenBy(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<StockLedger> AddAsync(StockLedger entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        _context.StockLedgers.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(StockLedger entity)
    {
        _context.StockLedgers.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.StockLedgers.FirstOrDefaultAsync(l => l.Id == id);
        if (entity == null) return;
        _context.StockLedgers.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
