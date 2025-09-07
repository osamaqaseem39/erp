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

    public async Task<ProductionRun?> GetByIdAsync(Guid id)
    {
        return await _context.ProductionRuns
            .Include(pr => pr.ProductionOrder)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<IEnumerable<ProductionRun>> GetAllAsync()
    {
        return await _context.ProductionRuns
            .Include(pr => pr.ProductionOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionRun>> GetByProductionOrderAsync(Guid productionOrderId)
    {
        return await _context.ProductionRuns
            .Include(pr => pr.ProductionOrder)
            .Where(pr => pr.ProductionOrderId == productionOrderId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionRun>> GetByStatusAsync(string status)
    {
        return await _context.ProductionRuns
            .Include(pr => pr.ProductionOrder)
            .Where(pr => pr.Status == status)
            .ToListAsync();
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

    public async Task<bool> DeleteAsync(Guid id)
    {
        var productionRun = await _context.ProductionRuns.FindAsync(id);
        if (productionRun == null) return false;

        _context.ProductionRuns.Remove(productionRun);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}