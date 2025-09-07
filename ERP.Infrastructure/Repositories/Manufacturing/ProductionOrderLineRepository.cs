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

    public async Task<ProductionOrderLine?> GetByIdAsync(Guid id)
    {
        return await _context.ProductionOrderLines
            .Include(pol => pol.ProductionOrder)
            .Include(pol => pol.Item)
            .FirstOrDefaultAsync(pol => pol.Id == id);
    }

    public async Task<IEnumerable<ProductionOrderLine>> GetAllAsync()
    {
        return await _context.ProductionOrderLines
            .Include(pol => pol.ProductionOrder)
            .Include(pol => pol.Item)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrderLine>> GetByProductionOrderAsync(Guid productionOrderId)
    {
        return await _context.ProductionOrderLines
            .Include(pol => pol.ProductionOrder)
            .Include(pol => pol.Item)
            .Where(pol => pol.ProductionOrderId == productionOrderId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrderLine>> GetByItemAsync(Guid itemId)
    {
        return await _context.ProductionOrderLines
            .Include(pol => pol.ProductionOrder)
            .Include(pol => pol.Item)
            .Where(pol => pol.ItemId == itemId)
            .ToListAsync();
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

    public async Task<bool> DeleteAsync(Guid id)
    {
        var productionOrderLine = await _context.ProductionOrderLines.FindAsync(id);
        if (productionOrderLine == null) return false;

        _context.ProductionOrderLines.Remove(productionOrderLine);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}