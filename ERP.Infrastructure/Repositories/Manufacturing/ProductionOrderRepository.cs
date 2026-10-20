using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public class ProductionOrderRepository : IProductionOrderRepository
{
    private readonly ERPDbContext _context;

    public ProductionOrderRepository(ERPDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProductionOrder> Query(Guid tenantId) =>
        _context.ProductionOrders
            .Include(po => po.Item)
            .Include(po => po.ProductionOrderLines)
            .Include(po => po.ProductionRuns)
            .Where(po => po.TenantId == tenantId);

    public async Task<ProductionOrder?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<IEnumerable<ProductionOrder>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionOrder>();
        return await Query(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrder>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionOrder>();
        return await Query(tenantId).Where(po => po.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrder>> GetByItemAsync(Guid itemId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionOrder>();
        return await Query(tenantId).Where(po => po.ItemId == itemId).ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrder>> GetByWorkCenterAsync(Guid workCenterId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionOrder>();
        return await Query(tenantId).Where(po => po.WorkCenterId == workCenterId).ToListAsync();
    }

    public async Task<ProductionOrder?> GetByNumberAsync(string productionOrderNumber, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(po => po.ProductionOrderNumber == productionOrderNumber);
    }

    public async Task<Guid> CreateAsync(ProductionOrder productionOrder)
    {
        _context.ProductionOrders.Add(productionOrder);
        await _context.SaveChangesAsync();
        return productionOrder.Id;
    }

    public async Task<bool> UpdateAsync(ProductionOrder productionOrder)
    {
        _context.ProductionOrders.Update(productionOrder);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var productionOrder = await GetByIdAsync(id, tenantId);
        if (productionOrder == null) return false;

        _context.ProductionOrders.Remove(productionOrder);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
