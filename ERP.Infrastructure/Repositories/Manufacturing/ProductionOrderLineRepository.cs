using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public class ProductionOrderLineRepository : IProductionOrderLineRepository
{
    private readonly ERPDbContext _context;

    public ProductionOrderLineRepository(ERPDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProductionOrderLine> Query(Guid tenantId) =>
        _context.ProductionOrderLines
            .Include(pol => pol.ProductionOrder)
            .Include(pol => pol.Item)
            .Where(pol => pol.TenantId == tenantId);

    public async Task<ProductionOrderLine?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(pol => pol.Id == id);
    }

    public async Task<IEnumerable<ProductionOrderLine>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionOrderLine>();
        return await Query(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrderLine>> GetByProductionOrderAsync(Guid productionOrderId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionOrderLine>();
        return await Query(tenantId).Where(pol => pol.ProductionOrderId == productionOrderId).ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrderLine>> GetByItemAsync(Guid itemId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionOrderLine>();
        return await Query(tenantId).Where(pol => pol.ItemId == itemId).ToListAsync();
    }

    public async Task<Guid> CreateAsync(ProductionOrderLine productionOrderLine)
    {
        _context.ProductionOrderLines.Add(productionOrderLine);
        await _context.SaveChangesAsync();
        return productionOrderLine.Id;
    }

    public async Task<bool> UpdateAsync(ProductionOrderLine productionOrderLine)
    {
        _context.ProductionOrderLines.Update(productionOrderLine);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var productionOrderLine = await GetByIdAsync(id, tenantId);
        if (productionOrderLine == null) return false;

        _context.ProductionOrderLines.Remove(productionOrderLine);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
