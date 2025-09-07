using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class PermissionRepository : IPermissionRepository
{
    private readonly ERPDbContext _context;

    public PermissionRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(Guid id)
    {
        return await _context.Permissions
            .Include(p => p.RolePermissions)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _context.Permissions
            .Include(p => p.RolePermissions)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetActiveAsync()
    {
        return await _context.Permissions
            .Include(p => p.RolePermissions)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetByModuleAsync(string module)
    {
        return await _context.Permissions
            .Include(p => p.RolePermissions)
            .Where(p => p.Module == module)
            .ToListAsync();
    }

    public async Task<Permission?> GetByNameAsync(string name)
    {
        return await _context.Permissions
            .Include(p => p.RolePermissions)
            .FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<Guid> CreateAsync(Permission permission)
    {
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();
        return permission.Id;
    }

    public async Task<bool> UpdateAsync(Permission permission)
    {
        _context.Permissions.Update(permission);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null) return false;

        _context.Permissions.Remove(permission);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}