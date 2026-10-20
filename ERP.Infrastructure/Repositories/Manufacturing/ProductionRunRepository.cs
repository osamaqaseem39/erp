using ERP.Domain.Manufacturing;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Manufacturing;

public class ProductionRunRepository : IProductionRunRepository
{
    private readonly ERPDbContext _context;

    public ProductionRunRepository(ERPDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProductionRun> Query(Guid tenantId) =>
        _context.ProductionRuns
            .Include(pr => pr.ProductionOrder)
            .Where(pr => pr.TenantId == tenantId);

    public async Task<ProductionRun?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<IEnumerable<ProductionRun>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionRun>();
        return await Query(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<ProductionRun>> GetByProductionOrderAsync(Guid productionOrderId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionRun>();
        return await Query(tenantId).Where(pr => pr.ProductionOrderId == productionOrderId).ToListAsync();
    }

    public async Task<IEnumerable<ProductionRun>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProductionRun>();
        return await Query(tenantId).Where(pr => pr.Status == status).ToListAsync();
    }

    public async Task<Guid> CreateAsync(ProductionRun productionRun)
    {
        _context.ProductionRuns.Add(productionRun);
        await _context.SaveChangesAsync();
        return productionRun.Id;
    }

    public async Task<bool> UpdateAsync(ProductionRun productionRun)
    {
        _context.ProductionRuns.Update(productionRun);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var productionRun = await GetByIdAsync(id, tenantId);
        if (productionRun == null) return false;

        _context.ProductionRuns.Remove(productionRun);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
