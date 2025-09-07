using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class TenantRepository : ITenantRepository
{
    private readonly ERPDbContext _context;

    public TenantRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        return await _context.Tenants
            .Include(t => t.Subscriptions)
            .Include(t => t.Companies)
            .Include(t => t.Users)
            .Include(t => t.Roles)
            .Include(t => t.AuditLogs)
            .Include(t => t.Settings)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await _context.Tenants
            .Include(t => t.Subscriptions)
            .Include(t => t.Companies)
            .Include(t => t.Users)
            .Include(t => t.Roles)
            .Include(t => t.AuditLogs)
            .Include(t => t.Settings)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tenant>> GetActiveAsync()
    {
        return await _context.Tenants
            .Include(t => t.Subscriptions)
            .Include(t => t.Companies)
            .Include(t => t.Users)
            .Include(t => t.Roles)
            .Include(t => t.AuditLogs)
            .Include(t => t.Settings)
            .Where(t => t.IsActive)
            .ToListAsync();
    }

    public async Task<Tenant?> GetByDomainAsync(string domain)
    {
        return await _context.Tenants
            .Include(t => t.Subscriptions)
            .Include(t => t.Companies)
            .Include(t => t.Users)
            .Include(t => t.Roles)
            .Include(t => t.AuditLogs)
            .Include(t => t.Settings)
            .FirstOrDefaultAsync(t => t.Domain == domain);
    }

    public async Task<Guid> CreateAsync(Tenant tenant)
    {
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        return tenant.Id;
    }

    public async Task<bool> UpdateAsync(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return false;

        _context.Tenants.Remove(tenant);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}