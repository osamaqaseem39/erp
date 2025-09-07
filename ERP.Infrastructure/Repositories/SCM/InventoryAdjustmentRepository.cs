using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.SCM;

public class InventoryAdjustmentRepository : IInventoryAdjustmentRepository
{
    private readonly ERPDbContext _context;

    public InventoryAdjustmentRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryAdjustment?> GetByIdAsync(int id)
    {
        return await _context.InventoryAdjustments.FindAsync(id);
    }

    public async Task<IEnumerable<InventoryAdjustment>> GetAllAsync()
    {
        return await _context.InventoryAdjustments.ToListAsync();
    }

    public async Task<InventoryAdjustment> AddAsync(InventoryAdjustment entity)
    {
        _context.InventoryAdjustments.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(InventoryAdjustment entity)
    {
        _context.InventoryAdjustments.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.InventoryAdjustments.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}