using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ERPDbContext _context;

    public SubscriptionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(Guid id)
    {
        return await _context.Subscriptions
            .Include(s => s.Tenant)
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Subscription>> GetAllAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.Tenant)
            .Include(s => s.Plan)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetByTenantAsync(Guid tenantId)
    {
        return await _context.Subscriptions
            .Include(s => s.Tenant)
            .Include(s => s.Plan)
            .Where(s => s.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetByStatusAsync(string status)
    {
        return await _context.Subscriptions
            .Include(s => s.Tenant)
            .Include(s => s.Plan)
            .Where(s => s.Status == status)
            .ToListAsync();
    }

    public async Task<Subscription?> GetActiveByTenantAsync(Guid tenantId)
    {
        return await _context.Subscriptions
            .Include(s => s.Tenant)
            .Include(s => s.Plan)
            .Where(s => s.TenantId == tenantId && s.Status == "Active")
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Guid> CreateAsync(Subscription subscription)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return subscription.Id;
    }

    public async Task<bool> UpdateAsync(Subscription subscription)
    {
        _context.Subscriptions.Update(subscription);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null) return false;

        _context.Subscriptions.Remove(subscription);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}