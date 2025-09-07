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

    public async Task<ProductionOrder?> GetByIdAsync(Guid id)
    {
        return await _context.ProductionOrders
            .Include(po => po.Item)
            .Include(po => po.ProductionOrderLines)
            .Include(po => po.ProductionRuns)
            .FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<IEnumerable<ProductionOrder>> GetAllAsync()
    {
        return await _context.ProductionOrders
            .Include(po => po.Item)
            .Include(po => po.ProductionOrderLines)
            .Include(po => po.ProductionRuns)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrder>> GetByStatusAsync(string status)
    {
        return await _context.ProductionOrders
            .Include(po => po.Item)
            .Include(po => po.ProductionOrderLines)
            .Include(po => po.ProductionRuns)
            .Where(po => po.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrder>> GetByItemAsync(Guid itemId)
    {
        return await _context.ProductionOrders
            .Include(po => po.Item)
            .Include(po => po.ProductionOrderLines)
            .Include(po => po.ProductionRuns)
            .Where(po => po.ItemId == itemId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrder>> GetByWorkCenterAsync(Guid workCenterId)
    {
        return await _context.ProductionOrders
            .Include(po => po.Item)
            .Include(po => po.ProductionOrderLines)
            .Include(po => po.ProductionRuns)
            .Where(po => po.WorkCenterId == workCenterId)
            .ToListAsync();
    }

    public async Task<ProductionOrder?> GetByNumberAsync(string productionOrderNumber)
    {
        return await _context.ProductionOrders
            .Include(po => po.Item)
            .Include(po => po.ProductionOrderLines)
            .Include(po => po.ProductionRuns)
            .FirstOrDefaultAsync(po => po.ProductionOrderNumber == productionOrderNumber);
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

    public async Task<bool> DeleteAsync(Guid id)
    {
        var productionOrder = await _context.ProductionOrders.FindAsync(id);
        if (productionOrder == null) return false;

        _context.ProductionOrders.Remove(productionOrder);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}