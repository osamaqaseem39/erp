using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class PlanRepository : IPlanRepository
{
    private readonly ERPDbContext _context;

    public PlanRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Plan?> GetByIdAsync(Guid id)
    {
        return await _context.Plans
            .Include(p => p.Subscriptions)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Plan>> GetAllAsync()
    {
        return await _context.Plans
            .Include(p => p.Subscriptions)
            .ToListAsync();
    }

    public async Task<IEnumerable<Plan>> GetActiveAsync()
    {
        return await _context.Plans
            .Include(p => p.Subscriptions)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<Plan?> GetByNameAsync(string planName)
    {
        return await _context.Plans
            .Include(p => p.Subscriptions)
            .FirstOrDefaultAsync(p => p.PlanName == planName);
    }

    public async Task<Guid> CreateAsync(Plan plan)
    {
        _context.Plans.Add(plan);
        await _context.SaveChangesAsync();
        return plan.Id;
    }

    public async Task<bool> UpdateAsync(Plan plan)
    {
        _context.Plans.Update(plan);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var plan = await _context.Plans.FindAsync(id);
        if (plan == null) return false;

        _context.Plans.Remove(plan);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}